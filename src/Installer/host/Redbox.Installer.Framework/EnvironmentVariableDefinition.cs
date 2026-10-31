using System;
using System.ComponentModel;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("Environment Variable")]
[Element(XPath = "env-variable")]
public class EnvironmentVariableDefinition : Definition
{
	private string m_name;

	private string m_value;

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

	[ElementProperty(Name = "value", IsRequired = true)]
	public string Value
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_value);
		}
		set
		{
			m_value = value;
		}
	}

	[DefaultValue(InstallMode.Install)]
	[ElementProperty(Name = "mode")]
	public InstallMode Mode { get; set; }

	[DefaultValue(EnvironmentVariableTarget.Machine)]
	[ElementProperty(Name = "target")]
	public EnvironmentVariableTarget Target { get; set; }

	public EnvironmentVariableDefinition()
	{
		Mode = InstallMode.Install;
		Target = EnvironmentVariableTarget.Machine;
	}

	public override int GetTaskCount()
	{
		if (Mode != InstallerProject.Instance.Mode)
		{
			return 0;
		}
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return $"Set environment variable {Target}:{Name}={Value}";
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			if (Mode != InstallMode.Install)
			{
				return;
			}
			Environment.SetEnvironmentVariable(Name, Value, Target);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to set environment variable {Name}={Value}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		if (Mode == InstallMode.Uninstall)
		{
			try
			{
				Environment.SetEnvironmentVariable(Name, Value, Target);
			}
			catch (Exception ex)
			{
				errors.Add(Error.NewError("I015", $"Unable to set environment variable {Name}={Value}; see details for exception.", ex.ToString()));
			}
			base.OnUninstall(progress, errors);
		}
	}
}
