using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel;

public static class KaseyaMethods
{
	public static string GetKaseyaIniDPath()
	{
		List<string> list = new List<string>();
		string text = "C:\\Program Files\\Kaseya";
		string text2 = "C:\\Program Files (x86)\\Kaseya";
		List<string> list2 = new List<string>();
		list2.AddRange(GetKaseyaInstallDirs(text));
		if (list2.Count == 0)
		{
			list2.AddRange(GetKaseyaInstallDirs(text2));
		}
		try
		{
			foreach (string item in list2)
			{
				string text3 = Path.Combine(item, "KaseyaD.ini");
				if (File.Exists(text3))
				{
					list.Add(text3);
				}
			}
			string text4 = list.OrderByDescending((string p) => File.GetLastWriteTimeUtc(p)).FirstOrDefault();
			LogHelper.Instance.Log("KaseyaD.ini path: {0}", text4);
			return text4;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("An unexpected exception was raised in GetKaseyainiPath function.", ex.Message);
		}
		return null;
	}

	private static List<string> GetKaseyaInstallDirs(string from)
	{
		List<string> list = new List<string>();
		if (Directory.Exists(from))
		{
			try
			{
				list.AddRange(Directory.GetDirectories(from, "RDBX*", SearchOption.TopDirectoryOnly));
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unexpected exception was raised in GetKaseyaInstallDirs function.", e);
			}
		}
		return list;
	}
}
