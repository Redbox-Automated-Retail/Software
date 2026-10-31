using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.Installer.Framework;
using Redbox.Macros;

namespace Redbox.Installer.Host;

public class Program : IDisposable
{
	private const string EventLogName = "Application";

	private const string EventSourceName = "Redbox Installer from XML Host";

	private readonly IConsole m_console;

	[STAThread]
	public static int Main()
	{
		if (!CheckApplicationPermissions())
		{
			MessageBox.Show("Redbox Installer from XML setup host executables must be run in a full trust environment.\n\nCopy the setup executable to a local drive and run it again.", "Redbox Installer from XML Host", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return 1;
		}
		if (!CheckFramework())
		{
			MessageBox.Show("Redbox Installer from XML setup host executables require the .NET 3.5 Framework.\n\nInstall the .NET 3.5 Framework and run setup again.", "Redbox Installer from XML Host", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return 1;
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		using Program program = new Program();
		return program.Run();
	}

	public int Run()
	{
		if (InstallerApplication.Instance.CommandLine.HelpRequested)
		{
			return 0;
		}
		ErrorList errorList = InstallerApplication.Instance.Initialize();
		if (ShowErrors(errorList))
		{
			return 1;
		}
		InstallerProject.Instance.ActiveConsole = m_console;
		if (InstallerApplication.Instance.Options.LogFile != null)
		{
			InstallerProject.Instance.ActiveConsole = new RedirectedConsole(InstallerApplication.Instance.Options.LogFile);
		}
		InstallerProject.Instance.LogGeneratedCode = InstallerApplication.Instance.Options.LogGeneratedSource;
		InstallerProject.Instance.Force = InstallerApplication.Instance.Options.Force;
		if (!InstallerApplication.Instance.Options.Silent)
		{
			Application.Run(MainForm.Instance);
			LaunchProcessesIfNecessary();
			return 0;
		}
		errorList.AddRange(InstallerProject.Instance.Execute(null));
		if (InstallerProject.Instance.ActiveConsole != m_console)
		{
			InstallerProject.Instance.ActiveConsole.Dispose();
		}
		if (!ShowErrors(errorList))
		{
			LaunchProcessesIfNecessary();
		}
		if (!errorList.ContainsError())
		{
			return 0;
		}
		return 1;
	}

	public void Dispose()
	{
		LogEvent($"Redbox Installer from XML host stopped at {DateTime.Now}" + MacroContainer.Instance.ExpandRuntimeMacros(" running script for ${ProductName} ${ProductVersion}."));
		if (m_console != null)
		{
			m_console.Dispose();
		}
		InstallerApplication.Instance.Dispose();
	}

	private Program()
	{
		m_console = new DefaultConsole();
		LogEvent($"Redbox Installer from XML host started at {DateTime.Now} by {WindowsIdentity.GetCurrent().Name}" + MacroContainer.Instance.ExpandRuntimeMacros(" running script for ${ProductName} ${ProductVersion}."));
	}

	private bool ShowErrors(ErrorList errors)
	{
		if (!errors.ContainsError())
		{
			return false;
		}
		if (!InstallerApplication.Instance.Options.Silent)
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Text = "Setup Errors";
			errorForm.Message = "Setup cannot proceed for the following reasons:";
			errorForm.Errors = errors;
			ErrorForm errorForm2 = errorForm;
			errorForm2.ShowDialog(MainForm.Instance);
		}
		else
		{
			if (!m_console.IsAttachedToConsole)
			{
				return true;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Setup cannot proceed for the following reasons:\n\n");
			foreach (Redbox.Installer.Framework.Error error in errors)
			{
				stringBuilder.AppendLine(error.ToString());
			}
			m_console.WriteLine(stringBuilder);
		}
		return true;
	}

	private static void LaunchProcessesIfNecessary()
	{
		if (InstallerApplication.Instance.UserCanceled || !(InstallerProject.Instance.Pages.GetByID("finish") is PageDefinition pageDefinition) || !pageDefinition.Properties.ContainsKey("LaunchOption") || !pageDefinition.Properties.ContainsKey("LaunchCheckState"))
		{
			return;
		}
		string strA = MacroContainer.Instance.ExpandRuntimeMacros(pageDefinition.Properties["LaunchCheckState"]);
		if (string.Compare(strA, bool.TrueString, ignoreCase: true) != 0)
		{
			return;
		}
		foreach (Definition launchOnlyProcess in InstallerProject.Instance.Definitions.GetLaunchOnlyProcesses())
		{
			launchOnlyProcess.Install(null, null);
		}
	}

	private static bool CheckApplicationPermissions()
	{
		try
		{
			new PermissionSet(PermissionState.Unrestricted).Demand();
			return true;
		}
		catch (SecurityException)
		{
			return false;
		}
	}

	private static bool CheckFramework()
	{
		return Directory.Exists(Environment.ExpandEnvironmentVariables("%windir%\\Microsoft.NET\\Framework\\v3.5"));
	}

	private static void LogEvent(string message)
	{
		try
		{
			using EventLog eventLog = new EventLog("Application");
			eventLog.Source = "Redbox Installer from XML Host";
			eventLog.WriteEntry(message, EventLogEntryType.Information);
		}
		catch
		{
		}
	}
}
