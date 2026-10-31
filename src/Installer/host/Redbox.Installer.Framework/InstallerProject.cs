using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

public class InstallerProject
{
	private Architecture m_machineArchitecture;

	private IConsole m_activeConsole;

	private List<string> m_externalFiles;

	private DefinitionList m_definitions;

	private List<Version> m_osVersions;

	public static InstallerProject Instance => Singleton<InstallerProject>.Instance;

	public string ID
	{
		get
		{
			return Values["id"];
		}
		set
		{
			Values["id"] = value;
		}
	}

	public string Name
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(Values["name"]);
		}
		set
		{
			Values["name"] = value;
		}
	}

	public string Icon
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(Values["icon"]);
		}
		set
		{
			Values["icon"] = value;
		}
	}

	public bool AllowModify
	{
		get
		{
			bool.TryParse(MacroContainer.Instance.ExpandRuntimeMacros(Values["allowmodify"]), out var result);
			return result;
		}
		set
		{
			Values["allowmodify"] = value.ToString();
		}
	}

	public InstallMode Mode
	{
		get
		{
			return Enum<InstallMode>.Parse(Values["installmode"], InstallMode.Install);
		}
		set
		{
			Values["installmode"] = value.ToString();
		}
	}

	public string InstallPath
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(Values["installpath"]);
		}
		set
		{
			Values["installpath"] = value;
		}
	}

	public string ProductName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(Values["productname"]);
		}
		set
		{
			Values["productname"] = value;
		}
	}

	public bool RebootRequired
	{
		get
		{
			bool.TryParse(MacroContainer.Instance.ExpandRuntimeMacros(Values["rebootrequiredflag"]), out var result);
			return result;
		}
		set
		{
			Values["rebootrequiredflag"] = value.ToString();
		}
	}

	public string ProductVersion
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(Values["productversion"]);
		}
		set
		{
			Values["productversion"] = value;
		}
	}

	public RebootMode RebootMode
	{
		get
		{
			return Enum<RebootMode>.ParseIgnoringCase(MacroContainer.Instance.ExpandRuntimeMacros(Values["rebootmode"]), RebootMode.IfNecessary);
		}
		set
		{
			Values["rebootmode"] = value.ToString();
		}
	}

	public bool IncludeUninstall
	{
		get
		{
			bool.TryParse(MacroContainer.Instance.ExpandRuntimeMacros(Values["uninstall"]), out var result);
			return result;
		}
		set
		{
			Values["uninstall"] = value.ToString();
		}
	}

	public SecurityLevel SecurityLevel
	{
		get
		{
			return Enum<SecurityLevel>.ParseIgnoringCase(MacroContainer.Instance.ExpandRuntimeMacros(Values["securitylevel"]), SecurityLevel.User);
		}
		set
		{
			Values["securitylevel"] = value.ToString();
		}
	}

	public DefinitionList Pages
	{
		get
		{
			if (Mode == InstallMode.Install)
			{
				return Definitions.GetPages();
			}
			PageDefinition pageDefinition = new PageDefinition();
			pageDefinition.ID = "welcome";
			pageDefinition.Type = PageType.Welcome;
			pageDefinition.Title = "Uninstall ${ProductName} ${ProductVersion}";
			pageDefinition.Text = "This wizard will uninstall the software package from your system.";
			PageDefinition pageDefinition2 = pageDefinition;
			PageDefinition pageDefinition3 = new PageDefinition();
			pageDefinition3.ID = "progress";
			pageDefinition3.Type = PageType.Progress;
			pageDefinition3.Title = "Uninstalling";
			pageDefinition3.SubTitle = "Setup is now removing the software package";
			pageDefinition3.ProceedText = "Please wait, setup will continue once uninstall steps are complete.";
			PageDefinition pageDefinition4 = pageDefinition3;
			PageDefinition pageDefinition5 = new PageDefinition();
			pageDefinition5.ID = "finish";
			pageDefinition5.Type = PageType.Finish;
			pageDefinition5.Title = "Uninstall Complete";
			pageDefinition5.Text = "Setup has completed uninstalling ${ProductName} ${ProductVersion}.";
			PageDefinition pageDefinition6 = pageDefinition5;
			return new DefinitionList(new PageDefinition[3] { pageDefinition2, pageDefinition4, pageDefinition6 });
		}
	}

	public StringDictionary Values => MacroContainer.Instance.Values;

	public int TotalNumberOfTasks => Definitions.GetTaskCount();

	public IConsole ActiveConsole
	{
		get
		{
			if (m_activeConsole == null)
			{
				m_activeConsole = new NullConsole();
			}
			return m_activeConsole;
		}
		set
		{
			m_activeConsole = value;
		}
	}

	public string DiskSpaceRequired => Int32Extensions.FormatAsByteSize(Definitions.GetSizeOfAllFiles());

	public DefinitionList Definitions
	{
		get
		{
			if (m_definitions == null)
			{
				m_definitions = new DefinitionList();
			}
			return m_definitions;
		}
	}

	public List<string> ExternalFiles
	{
		get
		{
			if (m_externalFiles == null)
			{
				m_externalFiles = new List<string>();
			}
			return m_externalFiles;
		}
	}

	public Architecture InstallArchitecture
	{
		get
		{
			return Enum<Architecture>.ParseIgnoringCase(MacroContainer.Instance.ExpandRuntimeMacros(Values["InstallArchitecture"]), Architecture.x86);
		}
		set
		{
			Values["InstallArchitecture"] = value.ToString();
		}
	}

	public Architecture MachineArchitecture
	{
		get
		{
			return m_machineArchitecture;
		}
		set
		{
			m_machineArchitecture = value;
			Values["MachineArchitecture"] = m_machineArchitecture.ToString();
		}
	}

	public IList<Version> OSRequired
	{
		get
		{
			if (m_osVersions == null)
			{
				List<Version> list = Constants.OperatingSystems.Keys.ToList();
				list.Sort();
				string text = Values["OSRequired"];
				if (!string.IsNullOrEmpty(text))
				{
					bool above = false;
					text = text.Replace(" ", string.Empty);
					if (text.EndsWith("+"))
					{
						above = true;
						text = text.Replace("+", string.Empty);
					}
					if (decimal.TryParse(text, out var result))
					{
						int num = Convert.ToInt32(result);
						int minor = Convert.ToInt32((result - (decimal)num) * 10m);
						Version version = new Version(num, minor);
						m_osVersions = list.SkipWhile((Version v) => v != version).TakeWhile((Version v) => v == version || above).ToList();
					}
				}
				if (m_osVersions == null)
				{
					m_osVersions = list;
				}
			}
			return m_osVersions;
		}
	}

	public bool LogGeneratedCode { get; set; }

	public bool Force { get; set; }

	public static ErrorList ParseXml(string xml, bool checkFiles, Assembly[] assemblies, out string outerXml, IConsole console)
	{
		ScriptParser scriptParser = new ScriptParser(null, console);
		return scriptParser.Compile(xml, assemblies, checkFiles, out outerXml);
	}

	public static ErrorList ParseFile(string fileName, bool checkFiles, Assembly[] assemblies, out string outerXml, IConsole console)
	{
		ScriptParser scriptParser = new ScriptParser(fileName, console);
		return scriptParser.Compile(null, assemblies, checkFiles, out outerXml);
	}

	public ErrorList Execute(Action<Definition> progress)
	{
		ErrorList errorList = new ErrorList();
		if (Force)
		{
			try
			{
				Directory.CreateDirectory(InstallPath);
			}
			catch (Exception e)
			{
				errorList.Add(Error.NewError("I019", $"Unable to force create the path '{InstallPath}'.", e));
			}
		}
		if (!Directory.Exists(InstallPath))
		{
			errorList.Add(Error.NewError("I019", $"The installation path '{InstallPath}' is not valid.", "Specify a valid installation path that exists on this machine and rerun setup."));
		}
		else
		{
			switch (Mode)
			{
			case InstallMode.Install:
				Definitions.SortByInstallExecutionOrder();
				Install(progress, errorList);
				ExecuteModuleInstaller(progress, errorList);
				break;
			case InstallMode.Uninstall:
				Definitions.SortByUninstallExecutionOrder();
				ExecuteModuleInstaller(progress, errorList);
				Uninstall(progress, errorList);
				break;
			}
			if (IncludeUninstall)
			{
				ConfigureUninstaller(errorList);
			}
		}
		return errorList;
	}

	public bool DoesCurrentUserMeetSecurityRequirements(ErrorList errors)
	{
		if (SecurityLevel == SecurityLevel.PowerUser && !SecurityHelper.IsCurrentUserPowerUser() && !SecurityHelper.IsCurrentUserAnAdministrator())
		{
			errors.Add(Error.NewError("I015", "The user that runs this installer must belong to the Power Users or Administrators security group.", "User does not belong to the BUILTIN\\Power Users group."));
			return false;
		}
		if (SecurityLevel == SecurityLevel.Administrator && !SecurityHelper.IsCurrentUserAnAdministrator())
		{
			errors.Add(Error.NewError("I015", "The user that runs this installer must belong to the Administrators security group.", "User does not belong to the BUILTIN\\Administrators group."));
			return false;
		}
		return true;
	}

	public bool ShouldAllowModify(ErrorList errors)
	{
		string text = string.Empty;
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(GetUninstallKeyWithoutHive());
		if (registryKey != null)
		{
			text = registryKey.GetValue("DisplayVersion") as string;
			registryKey.Close();
		}
		if (Mode == InstallMode.Install && !AllowModify && !string.IsNullOrEmpty(text))
		{
			errors.Add(Error.NewError("I015", string.Format(MacroContainer.Instance.ExpandRuntimeMacros("${ProductName} {0} is already installed on this system."), text), "Use Control Panel|Add/Remove Programs to remove the currently installed version and then run setup again."));
			return false;
		}
		return true;
	}

	internal void Initialize()
	{
		IDictionary environmentVariables = Environment.GetEnvironmentVariables();
		foreach (string key in environmentVariables.Keys)
		{
			Values[key] = (string)environmentVariables[key];
		}
		foreach (Environment.SpecialFolder value in Enum<Environment.SpecialFolder>.GetValues())
		{
			Values[value.ToString()] = Environment.GetFolderPath(value);
		}
		m_machineArchitecture = DetermineMachineArchitecture();
		Values["MachineArchitecture"] = m_machineArchitecture.ToString();
		string format = Environment.ExpandEnvironmentVariables("%windir%\\microsoft.net\\framework\\{0}");
		Values["dotnetfx10-path"] = string.Format(format, "v1.0.3705");
		Values["dotnetfx11-path"] = string.Format(format, "v1.1.4322");
		if (m_machineArchitecture == Architecture.x64)
		{
			format = Environment.ExpandEnvironmentVariables("%windir%\\microsoft.net\\framework64\\{0}");
		}
		Values["dotnetfx20-path"] = string.Format(format, "v2.0.50727");
		Values["dotnetfx30-path"] = string.Format(format, "v3.0");
		Values["dotnetfx35-path"] = string.Format(format, "v3.5");
		Values["dotnetfx40-path"] = string.Format(format, "v4.0.30319");
	}

	internal void Install(Action<Definition> progress, ErrorList errors)
	{
		using LibraryModule module = new LibraryModule(Assembly.GetExecutingAssembly().Location);
		Definitions.Install(module, progress, errors);
	}

	internal void Uninstall(Action<Definition> progress, ErrorList errors)
	{
		Definitions.Uninstall(progress, errors);
	}

	internal void ExecuteModuleInstaller(Action<Definition> progress, ErrorList errors)
	{
		List<string> list = new List<string>();
		foreach (string key in Values.Keys)
		{
			list.Add($"/{key}=\"{MacroContainer.Instance.ExpandRuntimeMacros(Values[key])}\"");
		}
		string[] args = list.ToArray();
		foreach (FileEntryDefinition loadableModule in Definitions.GetLoadableModules())
		{
			loadableModule.InstallModule(Mode, args, errors);
			progress?.Invoke(loadableModule);
		}
	}

	internal void ConfigureUninstaller(ErrorList errors)
	{
		string uninstallKey = GetUninstallKey();
		if (Mode == InstallMode.Uninstall)
		{
			try
			{
				Registry.LocalMachine.DeleteSubKeyTree(GetUninstallKeyWithoutHive());
				return;
			}
			catch (Exception ex)
			{
				errors.Add(Error.NewWarning("I018", "Unable to remove Add/Remove programs entry from registry.", ex.ToString()));
				return;
			}
		}
		string location = Assembly.GetExecutingAssembly().Location;
		string text = Path.Combine(InstallPath, Path.GetFileName(location));
		try
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			File.Copy(location, text);
		}
		catch (Exception ex2)
		{
			errors.Add(Error.NewError("I018", "Unable to copy setup host to installation path.", ex2.ToString()));
		}
		try
		{
			Registry.SetValue(uninstallKey, "DisplayName", $"{ProductName} {ProductVersion}");
			Registry.SetValue(uninstallKey, "DisplayVersion", ProductVersion);
			Registry.SetValue(uninstallKey, "InstallDate", DateTime.Now.ToString());
			Registry.SetValue(uninstallKey, "InstallPath", InstallPath);
			Registry.SetValue(uninstallKey, "Comments", string.Empty);
			Registry.SetValue(uninstallKey, "SilentUninstallString", $"{text} --remove --silent -D:InstallPath=\"{InstallPath}\"");
			Registry.SetValue(uninstallKey, "UninstallString", $"{text} --remove -D:InstallPath=\"{InstallPath}\"");
			Registry.SetValue(uninstallKey, "DisplayIcon", $"{text},0");
		}
		catch (Exception ex3)
		{
			errors.Add(Error.NewError("I018", "Unable to create Add/Remove programs entry in registry.", ex3.ToString()));
		}
	}

	internal string GetUninstallKey()
	{
		return $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{ID}";
	}

	internal string GetUninstallKeyWithoutHive()
	{
		string uninstallKey = GetUninstallKey();
		return uninstallKey.Substring(uninstallKey.IndexOf("\\") + 1);
	}

	private InstallerProject()
	{
		Initialize();
	}

	private static Architecture DetermineMachineArchitecture()
	{
		NativeMethods.SYSTEM_INFO lpSystemInfo = default(NativeMethods.SYSTEM_INFO);
		NativeMethods.GetNativeSystemInfo(ref lpSystemInfo);
		return lpSystemInfo.ToArchitecture();
	}
}
