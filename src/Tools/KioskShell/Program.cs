using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Outerwall.Shell;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Log.Framework;
using Redbox.Shell.Properties;
using Redbox.Shell.Remoting;

namespace Redbox.Shell;

public class Program
{
	private static ILogger logger = null!; // set by Main

	private static CommandLine? commandLine;

	private static CommandLine CommandLine
	{
		get
		{
			if (commandLine == null)
			{
				commandLine = CommandLine.ParseTo(Options.Instance);
			}
			return commandLine;
		}
	}

	[STAThread]
	public static int Main()
	{
#if !NETFRAMEWORK
		NetFrameworkCompatibility.ImportExeConfig();
#endif
		logger = LogHelper.Instance.CreateLog4NetLogger(typeof(Program));
		logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "shell-log4net.config"));
		LogHelper.Instance.Logger = logger;
		LogHelper.Instance.Log("Shell starting up...");
		AppDomain.CurrentDomain.UnhandledException += DomainExceptionHandler;
		Application.ThreadException += ApplicationExceptionHandler;
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
		KioskEngineService.Instance.Initialize(Settings.Default.KioskEnginePath, Settings.Default.KioskEngineIPCUrl);
		ApplySystemParameters();
		string startupFile = Settings.Default.StartupFile;
		if (!string.IsNullOrEmpty(startupFile) && File.Exists(startupFile))
		{
			ExecuteShellStartupFile(startupFile);
		}
		StringCollection startupFileMask = Settings.Default.StartupFileMask;
		List<string> searchPatterns = ((startupFileMask == null || startupFileMask.Count == 0) ? new List<string> { "*.lnk" } : startupFileMask.ToList());
		List<string> startupFiles = (from sd in Settings.Default.StartupDirectories.Cast<string>().Where(Directory.Exists)
			from sp in searchPatterns
			from f in Directory.GetFiles(sd, sp)
			select f).ToList();
		IEnumerable<string> startupExclusions = Settings.Default.StartupExclusions.Cast<string>();
		List<string> excludedFiles = startupFiles.Where((string f) => startupExclusions.All((string e) => f.IndexOf(e, StringComparison.OrdinalIgnoreCase) == -1)).ToList();
		excludedFiles.ForEach(delegate(string ef)
		{
			LogHelper.Instance.Log("Skipping \"{0}\" since file matches exclusion list", ef);
		});
		startupFiles.Except(excludedFiles).ToList().ForEach(ExecuteWindowsStartupFile);
		Application.Run(new Shell());
		return 0;
	}

	private static void DomainExceptionHandler(object sender, UnhandledExceptionEventArgs e)
	{
		try
		{
			Exception e2 = (Exception)e.ExceptionObject;
			LogHelper.Instance.Log("Unhandled Application Exception. Exiting Application", e2);
		}
		finally
		{
			Application.Exit();
		}
	}

	private static void ApplicationExceptionHandler(object sender, ThreadExceptionEventArgs e)
	{
		try
		{
			LogHelper.Instance.Log("Unhandled Windows Forms Thread Exception. Exiting Application", e.Exception);
		}
		finally
		{
			Application.Exit();
		}
	}

	private static void ApplySystemParameters()
	{
		try
		{
			string text = Settings.Default.Wallpaper ?? string.Empty;
			if (string.IsNullOrEmpty(text))
			{
				LogHelper.Instance.Log("The wallpaper cannot be set because no file was supplied in the configuration file.");
				return;
			}
			if (!File.Exists(text))
			{
				LogHelper.Instance.Log($"The wallpaper cannot be set because the file:\"{text}\" cannot be found.");
				return;
			}
			SystemParametersUtility.SetWallpaper(text, WallpaperStyle.FillScreen, TileWallpaper.NotTiled);
			LogHelper.Instance.Log($"The current user's wallpaper was set to:\"{text}\".");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to apply user system parameters.", e);
		}
	}

	private static void ExecuteShellStartupFile(string filename)
	{
		try
		{
			LogHelper.Instance.Log("Executing start up file:\"{0}\"", filename);
			using Process process = new Process();
			ProcessStartInfo startInfo = new ProcessStartInfo(filename)
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			process.StartInfo = startInfo;
			process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs args)
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					LogHelper.Instance.Log($"stdout: {args.Data}");
				}
			};
			process.Start();
			process.BeginOutputReadLine();
			string errorOutput = process.StandardError.ReadToEnd();
			if (!string.IsNullOrEmpty(errorOutput))
			{
				LogHelper.Instance.Log($"stderr: {errorOutput}");
			}
			process.WaitForExit();
			string backupName = string.Format("{0}.{1}.bak", filename, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
			LogHelper.Instance.Log("Moving start up file to:\"{0}\"", backupName);
			File.Move(filename, backupName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An error occurred executing start up file:\"{filename}\"", e);
		}
	}

	private static void ExecuteWindowsStartupFile(string filename)
	{
		string fileName = Path.GetFileName(filename);
		try
		{
			using Process process = new Process();
			LogHelper.Instance.Log("Executing start up file:\"{0}\"", fileName);
			ProcessStartInfo startInfo = new ProcessStartInfo(filename)
			{
				UseShellExecute = true
			};
			process.StartInfo = startInfo;
			process.Start();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An error occurred executing start up file:\"{fileName}\"", e);
		}
	}
}
