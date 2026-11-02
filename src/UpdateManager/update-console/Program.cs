using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.JSONPrettyPrinter;
using Redbox.Log.Framework;
using Redbox.Macros;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Tool.Properties;

namespace Redbox.UpdateManager.Tool;

public class Program
{
	private const string ApplicationId = "{4C9D4DA5-8E6C-40a0-867C-65E462A71ADB}";

	private static NamedLock? m_instanceLock;

	private static CommandLine? m_commandLine;

	private static readonly PropertyDictionary m_properties = new PropertyDictionary();

	private static readonly ILogger m_logger = LogHelper.Instance.CreateLog4NetLogger(typeof(Program));

	private static CommandLine CommandLine
	{
		get
		{
			if (m_commandLine == null)
			{
				m_commandLine = CommandLine.ParseTo(Opts);
			}
			return m_commandLine;
		}
	}

	private static Options Opts => Singleton<Options>.Instance;

	public static int Main(string[] args)
	{
#if !NETFRAMEWORK
		Redbox.Core.NetFrameworkCompatibility.ImportExeConfig();
#endif
		ErrorList errorList = new ErrorList();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		try
		{
			m_instanceLock = new NamedLock("{4C9D4DA5-8E6C-40a0-867C-65E462A71ADB}", LockScope.Local);
			if (!m_instanceLock.Exists())
			{
				Console.Error.WriteLine("Update Console is already running.");
				return -1;
			}
			m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "update-console-log4net.config"));
			LogHelper.Instance.Logger = m_logger;
			LogHelper.Instance.Log("Executing command line: {0}", System.Environment.CommandLine);
			Console.Out.WriteLine("Lock Gained for Process Id: {0} under user: {1}\\{2}", Process.GetCurrentProcess().Id, System.Environment.UserDomainName, System.Environment.UserName);
			if (CommandLine.Errors.Count > 0 && !CommandLine.HelpRequested)
			{
				CommandLine.WriteUsage(includeHelp: false);
				Console.Error.WriteLine("Command Line errors forcing exit.");
				return 1;
			}
			if (CommandLine.HelpRequested)
			{
				CommandLine.WriteUsage(includeHelp: true);
				return 0;
			}
			string? storeNumber = Opts.StoreNumber;
			if (Opts.ResetAll || Opts.ResetBITS || Opts.ResetManifest)
			{
				storeNumber = "0";
			}
			else if (string.IsNullOrEmpty(Opts.StoreNumber))
			{
				if (string.IsNullOrEmpty(Opts.KaseyaId))
				{
					CommandLine.WriteUsage(includeHelp: false);
					return 1;
				}
				string kaseyaMachineName = Opts.KaseyaId!.Substring(0, Opts.KaseyaId.IndexOf('.'));
				storeNumber = (kaseyaMachineName.StartsWith("redbox") ? kaseyaMachineName.Substring(7) : kaseyaMachineName);
			}
			LogHelper.Instance.Log("Starting store updater with store number: {0}", storeNumber);
			Updater updater = new Updater
			{
				StoreNumber = storeNumber
			};
			string updateServiceUrl = (string.IsNullOrEmpty(Opts.UpdateServiceUrl) ? Settings.Default.UpdateServiceUrl : Opts.UpdateServiceUrl!);
			errorList.AddRange(updater.Initialize(updateServiceUrl));
			if (errorList.ContainsError())
			{
				errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error error)
				{
					LogHelper.Instance.Log("{0} Details: {1}", error, error.Details);
				});
				return 1;
			}
			if (Opts.ResetAll)
			{
				updater.Reset();
				return 0;
			}
			if (Opts.ResetBITS)
			{
				updater.ResetBITS();
				return 0;
			}
			if (Opts.ResetManifest)
			{
				updater.ResetManifest();
				return 0;
			}
			if (!string.IsNullOrEmpty(Opts.Script))
			{
				string fullPath = Path.GetFullPath(m_properties.ExpandProperties(Opts.Script, Location.UnknownLocation)!); // Script checked above; GetFullPath throws on null anyway
				if (!File.Exists(fullPath))
				{
					Console.Error.WriteLine("{0} does not exists.", fullPath);
					return -1;
				}
				LogHelper.Instance.Log("Executing script {0}.", fullPath);
				IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
				try
				{
					kernelService.ExecuteChunk(File.ReadAllText(fullPath), reset: true);
				}
				catch (Exception e)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E99", "Unhandled exception in ExecuteChunk.", e));
				}
				finally
				{
					if (!kernelService.ScriptCompleted)
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("S99", "Script failed to complete.", "Check logs."));
					}
				}
			}
			else
			{
				if (Opts.PrintUnfinishedChanges)
				{
					IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
					foreach (string repositoryName in repositoryService.GetAllRepositories())
					{
						Console.Out.WriteLine(repositoryService.GetUnfinishedChanges(repositoryName).ToJson());
					}
				}
				if (Opts.Update)
				{
					if (!string.IsNullOrEmpty(Opts.Tree))
					{
						List<IRevLog> pendingChangeSets = updater.GetPendingChangeSets(Opts.Tree!);
						Console.Out.WriteLine(pendingChangeSets.ToJson());
						if (string.IsNullOrEmpty(Opts.Hash))
						{
							errorList.AddRange(updater.UpdateTo(Opts.Tree!));
						}
						else
						{
							errorList.AddRange(updater.UpdateTo(Opts.Tree!, Opts.Hash!));
						}
					}
					else
					{
						IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
						foreach (string repositoryName in repositoryService.GetAllRepositories())
						{
							repositoryService.UpdateToHead(repositoryName);
						}
					}
				}
				if (Opts.Activate)
				{
					if (!string.IsNullOrEmpty(Opts.Tree))
					{
						if (string.IsNullOrEmpty(Opts.Hash))
						{
							errorList.AddRange(updater.Activate(Opts.Tree!));
						}
						else
						{
							errorList.AddRange(updater.Activate(Opts.Tree!, Opts.Hash!));
						}
					}
					else
					{
						IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
						foreach (string repositoryName in repositoryService.GetAllRepositories())
						{
							errorList.AddRange(repositoryService.ActivateToHead(repositoryName, out var _));
						}
					}
				}
				if (Opts.Diff)
				{
					List<string> manifests = updater.GetManifests();
					Dictionary<string, object> changeSetsByManifest = new Dictionary<string, object>();
					foreach (string manifest in manifests)
					{
						List<IRevLog> pendingChangeSets = updater.GetPendingChangeSets(manifest);
						pendingChangeSets.Reverse();
						changeSetsByManifest.Add(manifest, pendingChangeSets);
					}
					Console.Out.WriteLine(changeSetsByManifest.ToJson());
				}
				if (Opts.List)
				{
					List<string> manifests = updater.GetManifests();
					Dictionary<string, object> changeSetsByManifest = new Dictionary<string, object>();
					foreach (string manifest in manifests)
					{
						List<IRevLog> appliedChangeSets = updater.GetAppliedChangeSets(manifest);
						appliedChangeSets.Reverse();
						changeSetsByManifest.Add(manifest, appliedChangeSets);
					}
					Console.Out.WriteLine(changeSetsByManifest.ToJson());
				}
				if (Opts.ListAll)
				{
					List<string> manifests = updater.GetManifests();
					Dictionary<string, object> changeSetsByManifest = new Dictionary<string, object>();
					foreach (string manifest in manifests)
					{
						List<IRevLog> appliedChangeSets = updater.GetAppliedChangeSets(manifest);
						appliedChangeSets.Reverse();
						changeSetsByManifest.Add(manifest, appliedChangeSets);
					}
					Console.Out.WriteLine(changeSetsByManifest.ToJson());
				}
				if (Opts.StartDownload)
				{
					errorList.AddRange(updater.StartUpdate());
				}
				if (Opts.Poll)
				{
					errorList.AddRange(updater.FinishUpdates());
					errorList.AddRange(updater.StartUpdate());
				}
				if (!string.IsNullOrEmpty(Opts.Dump))
				{
					if (string.IsNullOrEmpty(Opts.RepositoryName))
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", "repository-name is required.", "Specify repository-name."));
						return 1;
					}
					IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
					if (!repositoryService.ContainsRepository(Opts.RepositoryName!))
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", "repository-name does not exist.", "Check Name."));
						return 1;
					}
					switch (Opts.Dump!.ToLower())
					{
					case "active":
						Console.Write(repositoryService.GetActiveRevision(Opts.RepositoryName!));
						break;
					case "current":
						Console.Write(repositoryService.GetStagedRevision(Opts.RepositoryName!));
						break;
					case "label":
						Console.Write(repositoryService.GetActiveLabel(Opts.RepositoryName!));
						break;
					case "revlog":
						Console.Write(PrettyPrinter.GetPrettyString(repositoryService.GetAllChanges(Opts.RepositoryName!).ToJson()));
						break;
					default:
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", "Dump file is invalid.", "Valid values: active, current, label, revlog"));
						return 1;
					}
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U999", "An unhandled exception occurred in Updater.Program.Main.", e2));
		}
		finally
		{
			stopwatch.Stop();
			try
			{
				errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error error)
				{
					LogHelper.Instance.Log("{0} Details: {1}", error, error.Details);
				});
				Console.Error.WriteLine(errorList.ToJson());
			}
			catch (Exception e3)
			{
				LogHelper.Instance.Log("Exception in Program.Main.finally.", e3);
			}
			IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
			LogHelper.Instance.Log("Lock Released for Process Id: {0} under user: {1}\\{2} at UTC: {3}", Process.GetCurrentProcess().Id, System.Environment.UserDomainName, System.Environment.UserName, DateTime.UtcNow);
			if (kernelService != null)
			{
				switch (kernelService.ShutdownType)
				{
				case ShutdownType.Reboot:
					ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.Force, ShutdownReason.FlagPlanned);
					break;
				case ShutdownType.Shutdown:
					ShutdownTool.Shutdown(ShutdownFlags.ShutDown | ShutdownFlags.Force, ShutdownReason.FlagPlanned);
					break;
				}
			}
			if (m_instanceLock != null)
			{
				m_instanceLock.Dispose();
			}
		}
		if (errorList != null && errorList.ContainsError())
		{
			return 1;
		}
		return 0;
	}
}
