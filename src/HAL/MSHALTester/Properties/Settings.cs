using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
public sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://127.0.0.1:7001")]
	public string CommunicationURL => (string)this["CommunicationURL"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("70000")]
	public int ImmediateMoveTimeout => (int)this["ImmediateMoveTimeout"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool CheckForRunningHAL => (bool)this["CheckForRunningHAL"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool AllowSessionDisplay => (bool)this["AllowSessionDisplay"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool UsbServiceDebug => (bool)this["UsbServiceDebug"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool TestCommOnStartup => (bool)this["TestCommOnStartup"];

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs\\Timeouts.log")]
	public string CountersFile
	{
		get
		{
			return (string)this["CountersFile"];
		}
		set
		{
			this["CountersFile"] = value;
		}
	}

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("5000")]
	public int RouterRelayPause => (int)this["RouterRelayPause"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool DisplayEngineeringTab => (bool)this["DisplayEngineeringTab"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("c:\\Program Files\\Redbox\\HALService\\video")]
	public string ImageDirectory => (string)this["ImageDirectory"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe")]
	public string TunerApplication => (string)this["TunerApplication"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("2500")]
	public int TunerStartPause => (int)this["TunerStartPause"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
