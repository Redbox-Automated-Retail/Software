using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.USB;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class CardReaderDetector
{
	public static bool IsDeviceServiceCardReaderAttached()
	{
		bool flag = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.IsCardReaderConnected ?? false;
		LogHelper.Instance.Log($"CardReaderDetector.IsDeviceServiceCardReaderAttached: {flag}");
		return flag;
	}

	public static bool IsUSBCardReaderAttached()
	{
		using (HidDeviceManager hidDeviceManager = new HidDeviceManager())
		{
			List<string> uSBCardReaderKeys = GetUSBCardReaderKeys();
			HidDevice[] array = hidDeviceManager.EnumerateDevices();
			for (int i = 0; i < array.Length; i++)
			{
				string devicePath = array[i].DevicePath.ToLower();
				string s = string.Format("{0},{1}", GetIdFromPath(devicePath, "vid_"), GetIdFromPath(devicePath, "pid_"));
				SHA256Managed sHA256Managed = new SHA256Managed();
				string item = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(s))));
				if (uSBCardReaderKeys.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static string GetIdFromPath(string devicePath, string name)
	{
		return devicePath.Substring(devicePath.IndexOf(name) + 4, 4);
	}

	private static List<string> GetUSBCardReaderKeys()
	{
		List<string> list = new List<string>();
		using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Redbox\\CRList"))
		{
			if (registryKey != null)
			{
				object value = registryKey.GetValue("List");
				if (value != null)
				{
					string[] array = value.ToString().Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						list.Add((string)registryKey.GetValue(array[i]));
					}
				}
				registryKey.Close();
			}
		}
		if (list.Count == 0)
		{
			LogHelper.Instance.Log("...No card readers have been defined for this kiosk.", LogEntryType.Info);
		}
		return list;
	}
}
