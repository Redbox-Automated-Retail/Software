using System.ComponentModel;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "log")]
[DisplayName("Log")]
public class LogDefinition : Definition
{
	private string m_message;

	[ElementProperty(Name = "message")]
	public string Message
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_message);
		}
		set
		{
			m_message = value;
		}
	}

	[DefaultValue(InstallMode.Install)]
	[ElementProperty(Name = "mode")]
	public InstallMode Mode { get; set; }

	public override string ToExecutionMessage()
	{
		if (InstallerProject.Instance.Mode != Mode)
		{
			return string.Empty;
		}
		return $"** {InstallerProject.Instance.Mode} Log: {Message}";
	}

	public override int GetTaskCount()
	{
		if (InstallerProject.Instance.Mode != Mode)
		{
			return 0;
		}
		return 1;
	}
}
