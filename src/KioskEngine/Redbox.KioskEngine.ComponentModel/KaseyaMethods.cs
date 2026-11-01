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
		List<string> iniPaths = new List<string>();
		string programFilesPath = "C:\\Program Files\\Kaseya";
		string programFilesX86Path = "C:\\Program Files (x86)\\Kaseya";
		List<string> installDirs = new List<string>();
		installDirs.AddRange(GetKaseyaInstallDirs(programFilesPath));
		if (installDirs.Count == 0)
		{
			installDirs.AddRange(GetKaseyaInstallDirs(programFilesX86Path));
		}
		try
		{
			foreach (string installDir in installDirs)
			{
				string iniPath = Path.Combine(installDir, "KaseyaD.ini");
				if (File.Exists(iniPath))
				{
					iniPaths.Add(iniPath);
				}
			}
			string newestIniPath = iniPaths.OrderByDescending((string p) => File.GetLastWriteTimeUtc(p)).FirstOrDefault();
			LogHelper.Instance.Log("KaseyaD.ini path: {0}", newestIniPath);
			return newestIniPath;
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
