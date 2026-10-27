using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.Shell.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.1.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>C:\\Documents and Settings\\All Users\\Start Menu\\Programs\\Startup</string>\r\n</ArrayOfString>")]
	public StringCollection StartupDirectories => (StringCollection)this["StartupDirectories"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("c:\\startup.bat")]
	public string StartupFile => (string)this["StartupFile"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>*.lnk</string>\r\n  <string>*.exe</string>\r\n  <string>*.bat</string>\r\n</ArrayOfString>")]
	public StringCollection StartupFileMask => (StringCollection)this["StartupFileMask"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>dvd</string>\r\n  <string>cleanup</string>\r\n</ArrayOfString>")]
	public StringCollection StartupExclusions => (StringCollection)this["StartupExclusions"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("Control, Alt + Enter")]
	public string AttentionSequene => (string)this["AttentionSequene"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("C:\\Program Files\\Redbox\\REDS\\Kiosk Engine\\bin\\kioskengine.exe")]
	public string KioskEnginePath => (string)this["KioskEnginePath"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("C:\\Redbox\\background.bmp")]
	public string Wallpaper => (string)this["Wallpaper"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://localhost:8004")]
	public string KioskEngineIPCUrl => (string)this["KioskEngineIPCUrl"];
}
