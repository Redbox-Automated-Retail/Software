using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace KioskAdmin;

public static class Program
{
	private static void Main(string[] args)
	{
		Console.WriteLine("Redbox KioskAdmin");
		Console.WriteLine("");
		Dictionary<string, string> dictList = ArgsToDictionary(args);
		if (dictList.IsOptionSet("setpasswords"))
		{
			SecurityService.Instance.Run();
			LogService.Instance.DumpLog();
		}
		if (dictList.IsOptionSet("runbiosasshell"))
		{
			string location = Assembly.GetExecutingAssembly().Location;
			int processId = ShellHelper.StartProcessAsShellUser(location, "-runbios");
			ProcessFunctions.WaitForProcess(processId, new TimeSpan(0, 5, 0));
		}
		if (dictList.IsOptionSet("runbios"))
		{
			DellBios.RunDellBios();
		}
		if (dictList.IsOptionSet("demoteuser"))
		{
			SecurityService.Instance.DemoteDefaultUser();
			LogService.Instance.DumpLog();
		}
		if (dictList.IsOptionSet("getbackground"))
		{
			LogService.Log("Current background", BackgroundImage.GetWallpaper());
		}
		if (dictList.IsOptionSet("setbackground"))
		{
			string option = dictList.GetOption("setbackground");
			LogService.Log("Set background", option);
			if (File.Exists(option))
			{
				BackgroundImage.SetWallpaper(option);
				LogService.Log("Set wallpaper", option);
				string name = WindowsIdentity.GetCurrent().Name;
				LogService.Log("Current User", name);
			}
			else
			{
				LogService.Log("error", $"Background path does not exist {option}");
			}
			LogService.Instance.DumpLog("SetBackground.txt");
		}
	}

	private static bool IsOptionSet(this string[] stringList, string option)
	{
		List<string> list = stringList.ToList();
		return list.Exists((string s) => s.Equals(option, StringComparison.CurrentCultureIgnoreCase));
	}

	private static bool IsOptionSet(this Dictionary<string, string> dictList, string option)
	{
		return dictList.ContainsKey(option);
	}

	private static string GetOption(this Dictionary<string, string> dictList, string option)
	{
		if (!dictList.IsOptionSet(option))
		{
			return string.Empty;
		}
		return dictList[option];
	}

	private static Dictionary<string, string> ArgsToDictionary(string[] args)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		List<string> list = args.ToList();
		string option = string.Empty;
		string value = string.Empty;
		bool hasValue = false;
		list.ForEach(delegate(string item)
		{
			if (hasValue)
			{
				result[option] = item.Trim();
				hasValue = false;
			}
			else
			{
				option = GetParam(item, out hasValue, out value);
				if (string.IsNullOrEmpty(item))
				{
					hasValue = false;
				}
				else if (!hasValue)
				{
					result[option] = "true";
				}
				else if (!string.IsNullOrEmpty(value))
				{
					result[option] = value;
					hasValue = false;
				}
				else
				{
					string.IsNullOrEmpty(value);
				}
			}
		});
		return result;
	}

	private static string GetParam(string item, out bool hasValue, out string value)
	{
		hasValue = false;
		value = string.Empty;
		string text = string.Empty;
		item = item.Trim();
		int length = item.Length;
		if (item.StartsWith("--"))
		{
			text = ((length <= 2) ? string.Empty : item.Substring(2));
		}
		else if (item.StartsWith("-") || item.StartsWith("/"))
		{
			text = ((length <= 1) ? string.Empty : item.Substring(1));
		}
		if (text.Length == 0)
		{
			return text;
		}
		length = text.Length;
		int num = text.IndexOf(':');
		if (num > -1)
		{
			hasValue = true;
			if (length > num + 1)
			{
				value = text.Substring(num + 1);
			}
			text = text.Substring(0, num);
		}
		length = text.Length;
		num = text.IndexOf('=');
		if (num > -1)
		{
			hasValue = true;
			if (length > num + 1)
			{
				value = text.Substring(num + 1);
			}
			text = text.Substring(0, num);
		}
		return text;
	}
}
