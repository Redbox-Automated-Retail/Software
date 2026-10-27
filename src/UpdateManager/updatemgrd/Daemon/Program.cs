using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Log.Framework;

namespace Redbox.UpdateManager.Daemon;

public class Program
{
	private static readonly ILogger m_logger = LogHelper.Instance.CreateLog4NetLogger(typeof(UpdateManagerService));

	public static void Main(string[] args)
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		try
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "updatemgrd-log4net.config"));
			LogHelper.Instance.Logger = m_logger;
			LogHelper.Instance.Log("Configured log4net");
		}
		catch (Exception ex)
		{
			string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\logs");
			Directory.CreateDirectory(text);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(DateTime.Now.ToString());
			stringBuilder.AppendLine("An unhandled exception occurred registering log4net.");
			stringBuilder.AppendLine(ex.Message);
			stringBuilder.AppendLine(ex.StackTrace);
			if (ex.InnerException != null)
			{
				stringBuilder.AppendLine(ex.InnerException.Message);
				stringBuilder.AppendLine(ex.InnerException.StackTrace);
			}
			File.AppendAllText(Path.Combine(text, "errors.log"), stringBuilder.ToString());
			return;
		}
		try
		{
			LogHelper.Instance.Log("Detecting console mode");
			bool console;
			using (CommandLine commandLine = CommandLine.ParseTo(Options.Instance))
			{
				if (commandLine.HelpRequested || commandLine.Errors.Count > 0)
				{
					return;
				}
				using DefaultConsole defaultConsole = new DefaultConsole();
				if (Options.Instance.Register)
				{
					if (ExecuteInstaller(install: true, defaultConsole))
					{
						defaultConsole.WriteLine("Service successfully registered.");
					}
					else
					{
						defaultConsole.WriteLine("Service was not registered.");
					}
					return;
				}
				if (Options.Instance.Unregister)
				{
					if (ExecuteInstaller(install: false, defaultConsole))
					{
						defaultConsole.WriteLine("Service successfully unregistered.");
					}
					else
					{
						defaultConsole.WriteLine("Service was not unregistered.");
					}
					return;
				}
				console = Options.Instance.Console;
			}
			UpdateManagerService updateManagerService = new UpdateManagerService();
			if (Debugger.IsAttached | console)
			{
				LogHelper.Instance.Log("Debugger.IsAttached({0} || ConsoleMode({1})", Debugger.IsAttached, console);
				LogHelper.Instance.Log("Starting service manually");
				updateManagerService.StartService();
			}
			else
			{
				LogHelper.Instance.Log("Starting service with ServiceBase.Run");
				ServiceBase.Run(new UpdateManagerService[1] { updateManagerService });
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occurred in Program.Main", e);
		}
	}

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Exception e2 = (Exception)e.ExceptionObject;
		LogHelper.Instance.Log("Unhandled CurrentDomain Exception. Exiting Application", e2);
	}

	private static bool ExecuteInstaller(bool install, IConsole console)
	{
		try
		{
			using AssemblyInstaller assemblyInstaller = new AssemblyInstaller(typeof(Program).Assembly, new string[0]);
			assemblyInstaller.UseNewContext = true;
			IDictionary dictionary = new Hashtable();
			try
			{
				if (install)
				{
					assemblyInstaller.Install(dictionary);
				}
				else
				{
					assemblyInstaller.Uninstall(dictionary);
				}
				assemblyInstaller.Commit(dictionary);
				return true;
			}
			catch (Exception value)
			{
				assemblyInstaller.Rollback(dictionary);
				console.WriteLine(value);
				return false;
			}
		}
		catch (Exception ex)
		{
			console.WriteLine(ex);
			LogHelper.Instance.Log("Unhandled exception occurred in Program.ExecuteInstaller", ex);
			return false;
		}
	}
}
