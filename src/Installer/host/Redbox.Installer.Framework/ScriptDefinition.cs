using System;
using System.ComponentModel;
using Redbox.Core;
using Redbox.ScriptingHost.Framework;

namespace Redbox.Installer.Framework;

[Element(XPath = "script")]
[DisplayName("Script")]
public class ScriptDefinition : Definition
{
	[DefaultValue(InstallMode.Install)]
	[ElementProperty(Name = "mode")]
	public InstallMode? Mode { get; set; }

	[ElementProperty(UseNodeText = true)]
	public string SourceText { get; set; }

	public override string ToExecutionMessage()
	{
		if (InstallerProject.Instance.Mode != Mode)
		{
			return string.Empty;
		}
		return $"Execute Script: {base.ID}";
	}

	public override int GetTaskCount()
	{
		if (InstallerProject.Instance.Mode != Mode)
		{
			return 0;
		}
		return 1;
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		if (Mode.HasValue)
		{
			InstallMode? mode = Mode;
			if (mode.GetValueOrDefault() != InstallMode.Install || !mode.HasValue)
			{
				goto IL_006d;
			}
		}
		using (VsaScriptingHost vsaScriptingHost = new VsaScriptingHost())
		{
			vsaScriptingHost.SetKernelObject("rix", m_scriptKernelObject);
			vsaScriptingHost.ExecuteScript(SourceText);
		}
		goto IL_006d;
		IL_006d:
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		if (!Mode.HasValue || Mode == InstallMode.Uninstall)
		{
			using VsaScriptingHost vsaScriptingHost = new VsaScriptingHost();
			vsaScriptingHost.SetKernelObject("rix", m_scriptKernelObject);
			vsaScriptingHost.ExecuteScript(SourceText);
		}
		base.OnUninstall(progress, errors);
	}
}
