using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://devtools01:7002")]
	public string UpdateServiceUrl => (string)this["UpdateServiceUrl"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("..\\.store\\.repository")]
	public string ManifestRoot => (string)this["ManifestRoot"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("..\\.store\\.data")]
	public string DataStoreRoot => (string)this["DataStoreRoot"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("60")]
	public int MinimumRetryDelay => (int)this["MinimumRetryDelay"];
}
