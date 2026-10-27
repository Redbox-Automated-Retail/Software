using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.Daemon.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://localhost:7004")]
	public string Url => (string)this["Url"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("3")]
	public int MaximumWorkerThreads => (int)this["MaximumWorkerThreads"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("..\\scripts")]
	public string ScriptLocation => (string)this["ScriptLocation"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("${REDS}\\Kiosk Engine\\bin\\kioskengine.exe")]
	public string KioskEnginePath => (string)this["KioskEnginePath"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool DeveloperMode => (bool)this["DeveloperMode"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool InitialSubscriptionState => (bool)this["InitialSubscriptionState"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string StoreNumber => (string)this["StoreNumber"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("3503")]
	public int TrimWorkingSetInterval => (int)this["TrimWorkingSetInterval"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("00:00:30")]
	public TimeSpan UpdateServiceTimeout => (TimeSpan)this["UpdateServiceTimeout"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("${Scripts}\\shutdown.lua")]
	public string ShutdownScriptPath => (string)this["ShutdownScriptPath"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("60")]
	public int MinimumRetryDelay => (int)this["MinimumRetryDelay"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("http://bundle-dev-02:8002")]
	public string WcfUpdateServiceUrl => (string)this["WcfUpdateServiceUrl"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://qa-updatesvc01:7002")]
	public string UpdateServiceUrl => (string)this["UpdateServiceUrl"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://localhost:8004")]
	public string KioskEngineUrl => (string)this["KioskEngineUrl"];
}
