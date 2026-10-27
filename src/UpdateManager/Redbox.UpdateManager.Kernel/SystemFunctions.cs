using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;

namespace Redbox.UpdateManager.Kernel;

public static class SystemFunctions
{
	[Flags]
	private enum MoveFileFlags
	{
		MOVEFILE_REPLACE_EXISTING = 1,
		MOVEFILE_COPY_ALLOWED = 2,
		MOVEFILE_DELAY_UNTIL_REBOOT = 4,
		MOVEFILE_WRITE_THROUGH = 8
	}

	[KernelFunction(Name = "kernel.systemreloadperusersystemparameters")]
	internal static bool ReloadPerUserSystemParamters()
	{
		if (!UpdatePerUserSystemParameters())
		{
			LogHelper.Instance.Log("Failed to update per user system parameters.", new Win32Exception(Marshal.GetLastWin32Error()));
			return false;
		}
		return true;
	}

	[KernelFunction(Name = "kernel.import_script")]
	internal static void Import(string path)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path);
		if (File.Exists(text))
		{
			KernelService.Instance.LuaRuntime.DoString(File.ReadAllText(text));
			return;
		}
		LogHelper.Instance.Log("{0} does not exists can not import.", text);
	}

	[KernelFunction(Name = "kernel.log")]
	internal static void Log(string message)
	{
		LogHelper.Instance.Log($"Lua Runtime|{message}");
	}

	[KernelFunction(Name = "kernel.logf")]
	internal static void Log(string message, object parms)
	{
		if (parms is LuaTable table)
		{
			List<object> list = new List<object>();
			ExpandTableList(table, list);
			LogHelper.Instance.Log($"Lua Runtime|{message}", list.ToArray());
		}
		else
		{
			Log(message);
		}
	}

	[KernelFunction(Name = "kernel.checkmutex")]
	internal static bool IsMutexActive(string name)
	{
		bool createdNew;
		using (new Mutex(initiallyOwned: false, name, out createdNew))
		{
		}
		return !createdNew;
	}

	[KernelFunction(Name = "kernel.getvirtualmemoryusage")]
	internal static double GetVirtualMemoryUsageInMB(string name)
	{
		Process[] processesByName = Process.GetProcessesByName(name);
		if (processesByName.Length != 0)
		{
			return (double)processesByName[0].VirtualMemorySize64 / 1048576.0;
		}
		return -1.0;
	}

	[KernelFunction(Name = "kernel.getphysicalmemoryusage")]
	internal static double GetPhysicalMemoryUsageInMB(string name)
	{
		Process[] processesByName = Process.GetProcessesByName(name);
		if (processesByName.Length != 0)
		{
			return (double)processesByName[0].WorkingSet64 / 1048576.0;
		}
		return -1.0;
	}

	[KernelFunction(Name = "kernel.movelockedfile")]
	internal static void MoveFileEx(string source, string destiaion)
	{
		MoveLockedFileSystemEntry(source, destiaion);
	}

	[KernelFunction(Name = "kernel.deletelockedfile")]
	internal static void MoveFileEx(string target)
	{
		DeleteLockedFileSystemEntry(target);
	}

	[KernelFunction(Name = "kernel.reboot")]
	internal static void Reboot()
	{
		KernelService.Instance.ShutdownType = ShutdownType.Reboot;
	}

	[KernelFunction(Name = "kernel.shutdown")]
	internal static void Shutdown()
	{
		KernelService.Instance.ShutdownType = ShutdownType.Shutdown;
	}

	[KernelFunction(Name = "kernel.wait")]
	internal static void Wait(int milliseconds)
	{
		AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
		autoResetEvent.WaitOne(milliseconds, exitContext: false);
		autoResetEvent.Close();
	}

	[KernelFunction(Name = "kernel.shouldreboot")]
	public static bool ShouldReboot()
	{
		TimeSpan timeSpan = TimeSpan.FromMilliseconds(System.Environment.TickCount & 0x7FFFFFFF);
		TimeSpan timeSpan2 = TimeSpan.FromHours(1.0);
		if (timeSpan < timeSpan2)
		{
			LogHelper.Instance.Log($"Not rebooting because this kiosk rebooted {timeSpan.TotalMinutes} minutes ago.", LogEntryType.Info);
			return false;
		}
		string text = (string)RegistryFunctions.GetKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "RebootTime");
		if (string.IsNullOrEmpty(text))
		{
			LogHelper.Instance.Log("WARNING: No reboot time was found in the registry.");
			return false;
		}
		if (DateTime.TryParse(text, out var result))
		{
			DateTime now = DateTime.Now;
			DateTime dateTime = result.AddMinutes(-1.0);
			DateTime dateTime2 = result.AddMinutes(1.0);
			if (now >= dateTime)
			{
				return now <= dateTime2;
			}
			return false;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.mutesound")]
	internal static void MuteSound()
	{
		uint dwVolume = 0u;
		waveOutSetVolume(IntPtr.Zero, dwVolume);
		midiOutSetVolume(IntPtr.Zero, dwVolume);
	}

	[KernelFunction(Name = "kernel.systemrenamecomputer")]
	internal static void RenameComputer(string name)
	{
		foreach (ManagementObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
		{
			ManagementBaseObject methodParameters = instance.GetMethodParameters("Rename");
			methodParameters.SetPropertyValue("Name", name);
			instance.InvokeMethod("Rename", methodParameters, new InvokeMethodOptions());
		}
	}

	[KernelFunction(Name = "kernel.getSerialNumber")]
	internal static string GetSerialNumber()
	{
		string result = string.Empty;
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");
			foreach (ManagementObject item in managementObjectSearcher.Get())
			{
				result = item["SerialNumber"] as string;
			}
		}
		catch
		{
		}
		return result;
	}

	[KernelFunction(Name = "kernel.getManufacturer")]
	internal static string GetManufacturer()
	{
		string result = string.Empty;
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");
			foreach (ManagementObject item in managementObjectSearcher.Get())
			{
				result = item["Manufacturer"] as string;
			}
		}
		catch
		{
		}
		return result;
	}

	[KernelFunction(Name = "kernel.getUtcdatetime")]
	internal static string GetUtcDateTime()
	{
		return DateTime.UtcNow.ToString();
	}

	[KernelFunction(Name = "kernel.setdatetime")]
	internal static bool SetDateTime(string datetime)
	{
		try
		{
			return TimeZoneFunctions.SetTime(DateTime.Parse(datetime));
		}
		catch (FormatException e)
		{
			LogHelper.Instance.Log("An unparsable DateTime string was passed to SetDateTime", e);
			return false;
		}
		catch (ArgumentNullException e2)
		{
			LogHelper.Instance.Log("A null DateTime string was passed to SetDateTime", e2);
			return false;
		}
		catch (Exception e3)
		{
			LogHelper.Instance.Log("Unhandled exception in kernel.setdatetime", e3);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.settimezonefromserializedstring")]
	internal static bool SetTimeZoneFromSerializedString(string serializedTimeZoneInfo)
	{
		try
		{
			return TimeZoneFunctions.SetTimeZone(TimeZoneInfo.FromSerializedString(serializedTimeZoneInfo));
		}
		catch (SerializationException e)
		{
			LogHelper.Instance.Log("The source parameter cannot be deserialized back into a TimeZoneInfo object.", e, LogEntryType.Error);
			return false;
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Unhandled exception in SetTimeZoneFromSerializedString", e2, LogEntryType.Error);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.settimezonefromid")]
	internal static bool SetTimeZoneFromid(string timeZoneInfoId)
	{
		try
		{
			return TimeZoneFunctions.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
		}
		catch (TimeZoneNotFoundException e)
		{
			LogHelper.Instance.Log("The time zone identifier specified by id was not found. This means that a registry key whose name matches id does not exist, or that the key exists but does not contain any time zone data.", e, LogEntryType.Error);
			return false;
		}
		catch (ArgumentNullException e2)
		{
			LogHelper.Instance.Log("The id parameter is null.", e2, LogEntryType.Error);
			return false;
		}
		catch (SecurityException e3)
		{
			LogHelper.Instance.Log("The process does not have the permissions required to read from the registry key that contains the time zone information.", e3, LogEntryType.Error);
			return false;
		}
		catch (InvalidTimeZoneException e4)
		{
			LogHelper.Instance.Log("The time zone identifier was found, but the registry data is corrupted.", e4, LogEntryType.Error);
			return false;
		}
		catch (Exception e5)
		{
			LogHelper.Instance.Log("Unhandled exception in SetTimeZoneFromid", e5, LogEntryType.Error);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.gettimezone")]
	internal static string GetTimeZone()
	{
		return TimeZoneInfo.Local.StandardName;
	}

	[KernelFunction(Name = "kernel.gettimezoneinfo")]
	internal static string GetTimeZoneInfo()
	{
		return TimeZoneInfo.Local.ToSerializedString();
	}

	[DllImport("Kernel32.dll")]
	private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

	[DllImport("user32.dll")]
	private static extern bool UpdatePerUserSystemParameters();

	[DllImport("WinMM.dll")]
	private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

	[DllImport("WinMM.dll")]
	private static extern int midiOutSetVolume(IntPtr hmo, uint dwVolume);

	private static void ExpandTableList(LuaTable table, ICollection<object> list)
	{
		foreach (object value in table.Values)
		{
			if (value is LuaTable)
			{
				ExpandTableList((LuaTable)value, list);
			}
			list.Add(value.ToString());
		}
	}

	private static void MoveLockedFileSystemEntry(string source, string destination)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(source);
		string text2 = service.ExpandProperties(destination);
		MoveFileFlags moveFileFlags = MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
		if (!Directory.Exists(text) && !Directory.Exists(text2))
		{
			moveFileFlags |= MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
		}
		MoveFileEx(text, text2, moveFileFlags);
	}

	private static void DeleteLockedFileSystemEntry(string target)
	{
		MoveFileEx(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(target), null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
	}
}
