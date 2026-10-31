using System;
using System.ComponentModel;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("Firewall Exemption")]
[Element(XPath = "firewall-exemption")]
public class FirewallExemptionDefinition : Definition
{
	private const string OpenPortProgramID = "HNetCfg.FwOpenPort";

	private const string FirewallManagerProgramID = "HNetCfg.FwMgr";

	private const string AuthorizedApplicationProgramID = "HNetCfg.FwAuthorizedApplication";

	private string m_name;

	private string m_port;

	private string m_protocolType;

	private string m_applicationPath;

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

	[ElementProperty(Name = "port")]
	public string Port
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_port);
		}
		set
		{
			m_port = value;
		}
	}

	[ElementProperty(Name = "type")]
	public string ProtocolType
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_protocolType);
		}
		set
		{
			m_protocolType = value;
		}
	}

	[ElementProperty(Name = "path")]
	public string ApplicationPath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_applicationPath);
		}
		set
		{
			m_applicationPath = value;
		}
	}

	[DefaultValue(15)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(8)]
	public override int UninstallSortOrder { get; set; }

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} firewall exemption '{1}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", Name);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		base.OnUninstall(progress, errors);
	}
}
