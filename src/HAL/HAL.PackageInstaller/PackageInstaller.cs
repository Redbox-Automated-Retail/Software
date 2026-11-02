using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using Microsoft.Win32;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;
using Redbox.HAL.Script.Framework;

namespace Redbox.HAL.PackageInstaller;

internal sealed class PackageInstaller : IDisposable
{
	private readonly string[] DefaultDatabases = new string[2] { "HALCounters.vdb3", "HALData.vdb3" };

	private readonly string ServiceInstallerBatchFile;

	private readonly TextWriter Writer;

	private readonly string InstallerLog;

	private readonly string InstallLogsDirectory;

	private readonly string ConfigurationFullPath;

	private readonly IRuntimeService RuntimeService;

	private readonly List<IPackageInstallHandler> PackageHandlers = new List<IPackageInstallHandler>();

	private readonly ErrorList InstallErrors = new ErrorList();

	private const int BackupsToKeep = 2;

	private const string KioskLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs";

	private const string DefaultConfiguration = "HAL.xml.default";

	private const string ConfigurationFileName = "hal.xml";

	private bool Disposed;

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		if (InstallErrors.ContainsError())
		{
			Writer.WriteLine("** Installation failed with the following errors **");
			InstallErrors.ForEach(delegate(Error error)
			{
				Writer.WriteLine("{0}: {1}: {2} ", error.Code, error.Description, error.Details);
			});
		}
		Writer.Dispose();
	}

	private PackageInstaller()
	{
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		ConfigurationFullPath = RuntimeService.RuntimePath("hal.xml");
		InstallLogsDirectory = RuntimeService.InstallPath("InstallLogs");
		if (!Directory.Exists(InstallLogsDirectory))
		{
			try
			{
				Directory.CreateDirectory(InstallLogsDirectory);
			}
			catch
			{
			}
		}
		InstallerLog = Path.Combine(InstallLogsDirectory, "Package-installer.log");
		try
		{
			Writer = new StreamWriter(InstallerLog, append: true);
		}
		catch
		{
			Writer = StreamWriter.Null;
		}
		ServiceInstallerBatchFile = Path.Combine(RuntimeService.AssemblyDirectory, "register-service.bat");
	}

	private void CreateDirectories()
	{
		List<string> list = new List<string>();
		using (new DisposeableList<string>(list))
		{
			list.Add("C:\\Program Files\\Redbox\\KioskLogs");
			list.Add(Path.Combine(RuntimeService.AssemblyDirectory, "data"));
			list.Add("c:\\Program Files\\Redbox\\HALService\\Video");
			list.ForEach(delegate(string directory)
			{
				try
				{
					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}
				}
				catch (Exception ex)
				{
					Writer.WriteLine("Unable to create directory: {0}", ex.Message);
				}
			});
		}
	}

	private void InstallDatabases()
	{
		Array.ForEach(DefaultDatabases, delegate(string fileName)
		{
			string databasePath = RuntimeService.RuntimePath($"data\\{fileName}");
#if NETFRAMEWORK
			if (File.Exists(databasePath))
#else
			// The .NET HAL keeps its tables in <name>.sqlite (converted from the VistaDB file on its first start).
			if (File.Exists(databasePath) || File.Exists(Path.ChangeExtension(databasePath, ".sqlite")))
#endif
			{
				Writer.WriteLine("The database {0} already exists - take no further action.", fileName);
				return;
			}
			string defaultPath = RuntimeService.RuntimePath($"{fileName}.default");
			string arg = $"copy {defaultPath} -> {databasePath}";
			try
			{
				File.Copy(defaultPath, databasePath);
				Writer.WriteLine("{0}: SUCCESS", arg);
			}
			catch (Exception ex)
			{
				InstallErrors.Add(Error.NewError("I001", "Copy failure", $"{arg}: FAILURE reason = {ex.Message}"));
			}
		});
	}

	private void BackupConfiguration()
	{
		RuntimeService.CreateBackup(ConfigurationFullPath, BackupAction.Copy);
		List<FileInfo> backups = new List<FileInfo>();
		using (new DisposeableList<FileInfo>(backups))
		{
			Array.ForEach(Directory.GetFiles(RuntimeService.AssemblyDirectory, "hal*.xml"), delegate(string bf)
			{
				FileInfo fileInfo = new FileInfo(bf);
				if (!fileInfo.Name.Equals("hal.xml", StringComparison.CurrentCultureIgnoreCase) && !fileInfo.Name.Equals("HAL.xml.default", StringComparison.CurrentCultureIgnoreCase))
				{
					backups.Add(fileInfo);
				}
			});
			backups.Sort((FileInfo x, FileInfo y) => x.CreationTime.CompareTo(y.CreationTime));
			if (backups.Count > 2)
			{
				for (int index = 0; index < backups.Count - 2; index++)
				{
					RuntimeService.SafeDelete(backups[index].FullName);
				}
			}
		}
	}

	private bool ServiceIsInstalled()
	{
		ServiceController[] services = ServiceController.GetServices();
		for (int i = 0; i < services.Length; i++)
		{
			if (services[i].ServiceName.Equals("halsvc$default", StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private void InstallService()
	{
		if (!File.Exists(ServiceInstallerBatchFile))
		{
			InstallErrors.Add(Error.NewError("I003", "No service batch file.", "The service install batch file is missing."));
			return;
		}
		if (ServiceIsInstalled())
		{
			Writer.WriteLine("Service already installed - don't need to register.");
			return;
		}
		using Process process = new Process();
		process.StartInfo.FileName = ServiceInstallerBatchFile;
		process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		process.Start();
		process.WaitForExit();
		int exitCode = process.ExitCode;
		if (exitCode == 0)
		{
			Writer.WriteLine("Register service was successful.");
			return;
		}
		Writer.WriteLine("Register service returned with code {0}", exitCode);
		InstallErrors.Add(Error.NewError("I004", "Service install failed.", $"The service install returned error code {exitCode}"));
	}

#if !NETFRAMEWORK
	// The runtime's native DLLs (coreclr.dll, e_sqlite3.dll, ...) are not assemblies; read from the PE header, so that no
	// native DLL is loaded (see CommandRepository.IsNetAssembly in Redbox.HAL.IPC.Framework).
	private static bool IsAssembly(string path)
	{
		try
		{
			using FileStream stream = File.OpenRead(path);
			using System.Reflection.PortableExecutable.PEReader reader = new System.Reflection.PortableExecutable.PEReader(stream);
			return reader.HasMetadata;
		}
		catch (Exception)
		{
			return false;
		}
	}
#endif
	private void RegisterHandlers()
	{
		Writer.WriteLine("Register package handlers");
		string[] files = Directory.GetFiles(RuntimeService.AssemblyDirectory, "*.dll");
		string[] excludedFiles = new string[4] { "CortexDecoder.dll", "Hasp_windows_110892.dll", "CortexDecoderHasp.dll", "CDHelper.dll" };
		foreach (string path in files)
		{
			string file = Path.GetFileName(path);
			if (!string.IsNullOrEmpty(Array.Find(excludedFiles, (string a) => a.Equals(file, StringComparison.CurrentCultureIgnoreCase))))
			{
				Writer.WriteLine(" Exclude {0} ( '{1}' ) from handler search", file, path);
				continue;
			}
#if !NETFRAMEWORK
			// A self-contained .NET installer has the whole runtime next to it, and an .exe's code is in <name>.dll; .NET
			// Framework scanned neither (only libraries are .dll there).
			if (file.StartsWith("System.", StringComparison.OrdinalIgnoreCase) || file.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || file.StartsWith("api-ms-", StringComparison.OrdinalIgnoreCase) || File.Exists(Path.ChangeExtension(path, ".runtimeconfig.json")) || !IsAssembly(path))
			{
				continue;
			}
#endif
			try
			{
				Assembly assembly = Assembly.LoadFrom(path);
				Type typeFromHandle = typeof(IPackageInstallHandler);
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (typeFromHandle.IsAssignableFrom(type) && !type.IsInterface)
					{
						IPackageInstallHandler packageInstallHandler = (IPackageInstallHandler)Activator.CreateInstance(type)!;
						PackageHandlers.Add(packageInstallHandler);
						packageInstallHandler.OnRegister();
					}
				}
			}
			catch (Exception ex)
			{
				Writer.WriteLine(" Unable to load assembly '{0}' to scan for installer handlers.", path);
				Writer.WriteLine(" Exception: {0}", ex.Message);
			}
		}
		Writer.WriteLine("There are {0} handlers registered.", PackageHandlers.Count);
	}

	private void OnUpgrade()
	{
		PackageHandlers.ForEach(delegate(IPackageInstallHandler handler)
		{
			handler.OnUpgrade(InstallErrors, Writer);
		});
	}

	private void OnNewInstall()
	{
		PackageHandlers.ForEach(delegate(IPackageInstallHandler handler)
		{
			handler.OnNewInstall(InstallErrors, Writer);
		});
	}

	private static void Main(string[] args)
	{
		ServiceLocator.Instance.AddService<ITableTypeFactory>(new TableTypeFactory());
		ServiceLocator.Instance.AddService<IFormattedLogFactoryService>(new FormattedLogFactory());
		ServiceLocator.Instance.AddService<IRuntimeService>(new RuntimeService());
		PackageInstaller installer = new PackageInstaller();
		try
		{
			if (installer.ServiceIsInstalled())
			{
				ServiceController serviceController = new ServiceController("halsvc$default");
				try
				{
					serviceController.Stop();
					serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30.0));
				}
				catch
				{
				}
			}
			installer.CreateDirectories();
			Platform platform = installer.RuntimeService.GetPlatform();
			string version = "1.0.0.x";
			try
			{
				version = typeof(PackageInstaller).Assembly.GetName().Version!.ToString();
			}
			catch
			{
				version = "1.0.0.x";
			}
			installer.Writer.WriteLine("HAL package installer version {0} running on {1} at {2} (platform = {3})", version, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), platform.ToString());
			if (Platform.Windows7 != platform)
			{
				installer.Writer.WriteLine("** ERROR ** This version is not supported on this platform ( {0} )", platform.ToString());
				return;
			}
			installer.InstallDatabases();
			if (installer.InstallErrors.ContainsError())
			{
				installer.Writer.WriteLine("** There were problems copying databases - ABORTING install: **");
				installer.InstallErrors.ForEach(delegate(Error error)
				{
					installer.Writer.WriteLine(error.ToString());
				});
				return;
			}
			ServiceLocator.Instance.AddService<IDataTableService>(new DataTableService(exclusive: false));
			ServiceLocator.Instance.AddService<IPersistentMapService>(new PersistentMapService());
			ConfigurationService configurationService = ConfigurationService.Make(installer.ConfigurationFullPath);
			ServiceLocator.Instance.AddService<IConfigurationService>(configurationService);
			configurationService.RegisterConfiguration(Configurations.Controller.ToString(), ControllerConfiguration.Instance);
			configurationService.RegisterConfiguration(Configurations.Camera.ToString(), BarcodeConfiguration.MakeNewInstance2());
			installer.RegisterHandlers();
			if (File.Exists(installer.ConfigurationFullPath))
			{
				installer.BackupConfiguration();
				installer.Writer.WriteLine("The configuration file is already present.");
				configurationService.LoadAndUpgrade(installer.InstallErrors);
				installer.OnUpgrade();
			}
			else
			{
				installer.Writer.WriteLine("Installing configuration from GAMP.");
				string defaultConfigPath = installer.RuntimeService.RuntimePath("HAL.xml.default");
				try
				{
					File.Copy(defaultConfigPath, installer.ConfigurationFullPath);
				}
				catch (Exception ex)
				{
					installer.InstallErrors.Add(Error.NewError("I004", "Configuration install failed.", $"Unable to move file '{defaultConfigPath}' - error: {ex.Message}"));
					return;
				}
				configurationService.LoadAndImport(installer.InstallErrors);
				if (installer.InstallErrors.ErrorCount == 0)
				{
					installer.OnNewInstall();
				}
				else
				{
					installer.Writer.WriteLine("** Gamp data import failed - the configuration file may be incomplete. **");
				}
			}
			string assemblyFile = Path.Combine(installer.RuntimeService.AssemblyDirectory, "halsvc-win32.exe");
#if !NETFRAMEWORK
			// A .NET service's .exe is a native launcher; its assembly (and version) is halsvc-win32.dll.
			assemblyFile = Path.ChangeExtension(assemblyFile, ".dll");
#endif
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\HAL");
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
				registryKey.SetValue("Product Version", assemblyName.Version!.ToString());
			}
			catch (Exception ex2)
			{
				installer.Writer.WriteLine("Unable to set service registry key.", ex2);
				installer.InstallErrors.Add(Error.NewError("I001", "Service registration failure", ex2));
			}
			installer.InstallService();
		}
		finally
		{
			if (installer != null)
			{
				((IDisposable)installer).Dispose();
			}
		}
	}
}
