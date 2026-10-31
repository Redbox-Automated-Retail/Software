using System;
using System.Management;
using Redbox.Core;

namespace Redbox.KioskEngine.Environment;

internal static class Windows10TouchScreen
{
	// The original ran "wmic path Win32_PnPEntity where ... call Enable|Disable"; wmic.exe is gone from Windows 11,
	// so the same WMI query and method are used directly.
	private const string TouchScreenQuery = "SELECT * FROM Win32_PnPEntity WHERE Name='HID-Compliant touch screen'";

	private const string ENABLE = "Enable";

	private const string DISABLE = "Disable";

	public static void Enable()
	{
		InvokeOnTouchScreens(ENABLE);
	}

	public static void Disable()
	{
		InvokeOnTouchScreens(DISABLE);
	}

	private static void InvokeOnTouchScreens(string wmiMethod)
	{
		try
		{
			LogHelper.Instance.Log("Windows10TouchScreen - Attempting to " + wmiMethod + " HID-compliant touch screen");
			using ManagementObjectSearcher searcher = new ManagementObjectSearcher(TouchScreenQuery);
			using ManagementObjectCollection devices = searcher.Get();
			int count = 0;
			foreach (ManagementObject device in devices)
			{
				using (device)
				{
					object result = device.InvokeMethod(wmiMethod, null);
					LogHelper.Instance.Log($"Windows10TouchScreen - {wmiMethod} on {device["DeviceID"]} returned {result}");
					count++;
				}
			}
			if (count == 0)
			{
				LogHelper.Instance.Log("Windows10TouchScreen - no HID-compliant touch screen found");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Exception trying to toggle HID touch screen device!", e);
		}
	}
}
