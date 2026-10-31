using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("Registry Entry")]
[Element(XPath = "registry-entry")]
public class RegistryEntryDefinition : Definition
{
	private class RegistryInfo
	{
		public RegistryValueKind Type { get; private set; }

		public int Size { get; private set; }

		public byte[] Value { get; private set; }

		private RegistryInfo(RegistryValueKind t, int s, byte[] v)
		{
			Type = t;
			Size = s;
			Value = v;
		}

		public static RegistryInfo build(Type type, string value)
		{
			switch (type.ToString())
			{
			case "System.Int32":
				return new RegistryInfo(RegistryValueKind.DWord, 4, BitConverter.GetBytes(int.Parse(value)));
			case "System.UInt32":
				return new RegistryInfo(RegistryValueKind.DWord, 4, BitConverter.GetBytes(uint.Parse(value)));
			default:
			{
				int s = (value.Length + 1) * Marshal.SystemDefaultCharSize;
				return new RegistryInfo(RegistryValueKind.String, s, new ASCIIEncoding().GetBytes(value));
			}
			}
		}
	}

	private string m_key;

	private string m_name;

	private string m_value;

	[DefaultValue("System.String")]
	[ElementProperty(Name = "type")]
	public Type Type { get; set; }

	[ElementProperty(Name = "key", IsRequired = true)]
	public string Key
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_key);
		}
		set
		{
			m_key = value;
		}
	}

	[ElementProperty(Name = "name")]
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

	[ElementProperty(Name = "action")]
	[DefaultValue(RegistryEntryAction.Add)]
	public RegistryEntryAction Action { get; set; }

	[DefaultValue(3)]
	public override int InstallSortOrder { get; set; }

	internal static NativeMethods.RegSAM SAMValue
	{
		get
		{
			Architecture architecture = DetermineRIXArchitecture();
			Architecture machineArchitecture = InstallerProject.Instance.MachineArchitecture;
			Architecture installArchitecture = InstallerProject.Instance.InstallArchitecture;
			if (architecture == Architecture.x86 && machineArchitecture == Architecture.x64 && installArchitecture == Architecture.x64)
			{
				return NativeMethods.RegSAM.AllAccess | NativeMethods.RegSAM.WOW64_64Key;
			}
			if (architecture == Architecture.x64 && machineArchitecture == Architecture.x64 && installArchitecture == Architecture.x86)
			{
				return NativeMethods.RegSAM.AllAccess | NativeMethods.RegSAM.WOW64_32Key;
			}
			return NativeMethods.RegSAM.AllAccess;
		}
	}

	internal RegistryHive Root
	{
		get
		{
			string text = Key.ToLower();
			if (text.StartsWith("hkey_local_machine"))
			{
				return RegistryHive.LocalMachine;
			}
			if (text.StartsWith("hkey_classes_root"))
			{
				return RegistryHive.ClassesRoot;
			}
			if (text.StartsWith("hkey_current_user"))
			{
				return RegistryHive.CurrentUser;
			}
			if (text.StartsWith("hkey_current_config"))
			{
				return RegistryHive.CurrentConfig;
			}
			if (text.StartsWith("hkey_users"))
			{
				return RegistryHive.Users;
			}
			throw new ApplicationException($"Invalid root for key: {Key}.");
		}
	}

	internal string KeyWithoutRoot => Key.Substring(Key.IndexOf("\\") + 1);

	public override string ToExecutionMessage()
	{
		return $"{((InstallerProject.Instance.Mode == InstallMode.Install) ? Action : RegistryEntryAction.Remove)} {Key}={Value}";
	}

	public override int GetTaskCount()
	{
		return 1;
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			if (Action == RegistryEntryAction.Remove)
			{
				RemoveKey();
			}
			else
			{
				AddKey();
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to edit registry entry {Key}\\{Name}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			RemoveKey();
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to edit registry entry {Key}\\{Name}; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}

	internal void AddKey()
	{
		UIntPtr phkResult = UIntPtr.Zero;
		if (NativeMethods.RegCreateKeyEx(Root, KeyWithoutRoot, 0, string.Empty, NativeMethods.RegOption.NonVolatile, SAMValue, new NativeMethods.SECURITY_ATTRIBUTES(), out phkResult, out var _) != 0)
		{
			throw new ApplicationException($"Unable to create registry key: {Key}");
		}
		RegistryInfo registryInfo = RegistryInfo.build(Type, Value);
		if (NativeMethods.RegSetValueEx(phkResult, Name, 0, registryInfo.Type, registryInfo.Value, registryInfo.Size) != 0)
		{
			throw new ApplicationException($"Unable to set value {Name}, {Value} on key: {Key}");
		}
		if (NativeMethods.RegCloseKey(phkResult) != 0)
		{
			throw new ApplicationException($"Unable to close key: {Key}");
		}
	}

	internal void RemoveKey()
	{
		UIntPtr phkResult = UIntPtr.Zero;
		if (NativeMethods.RegOpenKeyEx(Root, KeyWithoutRoot, 0u, SAMValue, out phkResult) != 0 || phkResult == UIntPtr.Zero)
		{
			throw new ApplicationException($"Unable to open key: {Key}");
		}
		if (NativeMethods.RegDeleteValue(phkResult, Name) != 0)
		{
			throw new ApplicationException(string.Format("Unable to delete value for key: {0}", Key, Name));
		}
		string lpClass = new string(' ', 1024);
		uint lpcbClass = 1024u;
		uint lpcSubKeys = 0u;
		uint lpcbMaxSubKeyLen = 0u;
		uint lpcbMaxClassLen = 0u;
		uint lpcValues = 0u;
		uint lpcbMaxValueNameLen = 0u;
		uint lpcbMaxValueLen = 0u;
		uint lpcbSecurityDescriptor = 0u;
		if (NativeMethods.RegQueryInfoKey(phkResult, out lpClass, out lpcbClass, IntPtr.Zero, out lpcSubKeys, out lpcbMaxSubKeyLen, out lpcbMaxClassLen, out lpcValues, out lpcbMaxValueNameLen, out lpcbMaxValueLen, out lpcbSecurityDescriptor, default(IntPtr)) != 0)
		{
			throw new ApplicationException($"Unable to query key info for key: {Key}");
		}
		if (lpcSubKeys == 0 && lpcValues == 0)
		{
			int num;
			try
			{
				num = NativeMethods.RegDeleteKeyEx(Root, KeyWithoutRoot, SAMValue, 0u);
			}
			catch (EntryPointNotFoundException)
			{
				num = NativeMethods.RegDeleteKey(Root, KeyWithoutRoot);
			}
			if (num != 0)
			{
				throw new ApplicationException($"Unable to delete key: {Key}");
			}
		}
		if (NativeMethods.RegCloseKey(phkResult) != 0)
		{
			throw new ApplicationException($"Unable to close key: {Key}");
		}
	}

	internal bool RemoveValue(RegistryKey hive, string rootKey)
	{
		if (Name != null)
		{
			RegistryKey registryKey = hive.OpenSubKey(rootKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
			if (registryKey != null)
			{
				registryKey.DeleteValue(Name, throwOnMissingValue: false);
				bool result = registryKey.GetValueNames().Length == 0 && registryKey.GetSubKeyNames().Length == 0;
				registryKey.Close();
				return result;
			}
		}
		return true;
	}

	private static Architecture DetermineRIXArchitecture()
	{
		int num = IntPtr.Size * 8;
		return num switch
		{
			32 => Architecture.x86, 
			64 => Architecture.x64, 
			_ => throw new ApplicationException($"Unknown rix architecture ({num} bit) found!"), 
		};
	}
}
