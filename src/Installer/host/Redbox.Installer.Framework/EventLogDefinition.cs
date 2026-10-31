using System;
using System.ComponentModel;
using System.Diagnostics;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "event-log")]
[DisplayName("Event Log")]
public class EventLogDefinition : Definition
{
	private string m_logName;

	private string m_applicationName;

	[ElementProperty(Name = "name", IsRequired = true)]
	public string LogName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_logName);
		}
		set
		{
			m_logName = value;
		}
	}

	[ElementProperty(Name = "application-name", IsRequired = true)]
	public string ApplicationName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_applicationName);
		}
		set
		{
			m_applicationName = value;
		}
	}

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} Event Log {1} for registered application source {2}", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", LogName, ApplicationName);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			string text = LogName;
			if (string.Compare("system", text, ignoreCase: true) == 0 || string.Compare("application", text, ignoreCase: true) == 0 || string.Compare("security", text, ignoreCase: true) == 0)
			{
				text = string.Empty;
			}
			EventLog.CreateEventSource(ApplicationName, text);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to create Event Log {LogName} registered to application {ApplicationName}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			EventLog.DeleteEventSource(ApplicationName);
			if (string.Compare("system", LogName, ignoreCase: true) != 0 && string.Compare("application", LogName, ignoreCase: true) != 0 && string.Compare("security", LogName, ignoreCase: true) != 0 && EventLog.Exists(LogName))
			{
				EventLog.Delete(LogName);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to delete Event Log {LogName} and registred application {ApplicationName}; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}
}
