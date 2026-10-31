using System;
using System.ComponentModel;
using System.Diagnostics;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "execute-process")]
[DisplayName("Execute Process")]
public class ExecuteProcessDefinition : Definition
{
	private string m_fileName;

	private string m_arguments;

	private string m_workingDirectory;

	[ElementProperty(Name = "use-shell")]
	public bool UseShell { get; set; }

	[DefaultValue(false)]
	[ElementProperty(Name = "report-error")]
	public bool ReportErrorOnExit { get; set; }

	[ElementProperty(Name = "filename", IsRequired = true)]
	public string FileName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_fileName);
		}
		set
		{
			m_fileName = value;
		}
	}

	[ElementProperty(Name = "mode")]
	[DefaultValue(InstallMode.Install)]
	public InstallMode Mode { get; set; }

	[ElementProperty(Name = "launch-only")]
	public bool LaunchOnly { get; set; }

	[ElementProperty(Name = "arguments")]
	public string Arguments
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_arguments);
		}
		set
		{
			m_arguments = value;
		}
	}

	[DefaultValue(true)]
	[ElementProperty(Name = "wait-for-exit")]
	public bool WaitForExit { get; set; }

	[ElementProperty(Name = "suppress-window")]
	public bool SuppressWindow { get; set; }

	[ElementProperty(Name = "working-dir")]
	public string WorkingDirectory
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_workingDirectory);
		}
		set
		{
			m_workingDirectory = value;
		}
	}

	[DefaultValue(9)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(1)]
	public override int UninstallSortOrder { get; set; }

	public ExecuteProcessDefinition()
	{
		Mode = InstallMode.Install;
	}

	public override string ToExecutionMessage()
	{
		return $"Execute '{FileName}'...";
	}

	public override int GetTaskCount()
	{
		if (LaunchOnly)
		{
			return 0;
		}
		return 1;
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		if (Mode == InstallMode.Install)
		{
			try
			{
				ExecuteProcess(errors);
			}
			catch (Exception ex)
			{
				errors.Add(Error.NewError("I015", $"Unable to execute process {FileName}; see details for exception.", ex.ToString()));
			}
			base.OnInstall(module, progress, errors);
		}
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		if (Mode == InstallMode.Uninstall)
		{
			try
			{
				ExecuteProcess(errors);
			}
			catch (Exception ex)
			{
				errors.Add(Error.NewError("I015", $"Unable to execute process {FileName}; see details for exception.", ex.ToString()));
			}
			base.OnUninstall(progress, errors);
		}
	}

	internal void ExecuteProcess(ErrorList errors)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo(FileName);
		processStartInfo.UseShellExecute = UseShell;
		ProcessStartInfo processStartInfo2 = processStartInfo;
		if (Arguments != null)
		{
			processStartInfo2.Arguments = Arguments;
		}
		if (WorkingDirectory != null)
		{
			processStartInfo2.WorkingDirectory = WorkingDirectory;
		}
		if (SuppressWindow)
		{
			processStartInfo2.CreateNoWindow = true;
			processStartInfo2.WindowStyle = ProcessWindowStyle.Hidden;
		}
		Process process = Process.Start(processStartInfo2);
		if (process == null || !WaitForExit)
		{
			return;
		}
		process.WaitForExit();
		if (ReportErrorOnExit)
		{
			int exitCode = process.ExitCode;
			if (exitCode != 0)
			{
				errors.Add(Error.NewError("P999", $"{FileName} returned exit code: {exitCode}", "Process exited with an error code."));
			}
		}
	}
}
