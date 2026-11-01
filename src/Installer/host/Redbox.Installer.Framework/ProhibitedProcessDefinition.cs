using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("Prohibited Process")]
[Element(XPath = "prohibit-process")]
public class ProhibitedProcessDefinition : Definition
{
	private bool m_kill;

	private string m_name;

	[ElementProperty(Name = "kill")]
	public bool Kill
	{
		get
		{
			if (!m_kill)
			{
				return InstallerProject.Instance.Definitions.GetInstallProgressPage() == null;
			}
			return true;
		}
		set
		{
			m_kill = value;
		}
	}

	[ElementProperty(Name = "name", IsRequired = true)]
	public string Name
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_name);
		}
		set
		{
			m_name = value;
		}
	}

	public string WindowTitle { get; set; }

	[DefaultValue(0)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(0)]
	public override int UninstallSortOrder { get; set; }

	public bool KillProcess()
	{
		int num = 0;
		Process[] processesByName = Process.GetProcessesByName(Name);
		if (processesByName.Length > 0)
		{
			foreach (Process process in processesByName)
			{
				process.Kill();
				if (!process.HasExited)
				{
					num++;
				}
			}
		}
		return num == processesByName.Length;
	}

	public bool IsProcessRunning()
	{
		Process[] processesByName = Process.GetProcessesByName(Name);
		if (processesByName.Length > 0)
		{
			foreach (Process process in processesByName)
			{
				if (!process.HasExited)
				{
					if (processesByName.Length > 1)
					{
						WindowTitle = process.MainWindowTitle + " [Multiple instances]";
					}
					else
					{
						WindowTitle = process.MainWindowTitle;
					}
					return true;
				}
			}
		}
		return false;
	}

	public override string ToExecutionMessage()
	{
		return $"Kill processes named {Name}";
	}

	public override int GetTaskCount()
	{
		return 1;
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			if (IsProcessRunning())
			{
				for (int i = 0; i < 10; i++)
				{
					if (KillProcess())
					{
						break;
					}
					Thread.Sleep(500);
				}
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to kill process {Name}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			if (IsProcessRunning())
			{
				for (int i = 0; i < 10; i++)
				{
					if (KillProcess())
					{
						break;
					}
					Thread.Sleep(500);
				}
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to kill process {Name}; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}
}
