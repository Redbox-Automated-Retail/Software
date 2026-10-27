using System;
using System.IO;

namespace KioskClientService.API.Services.Configuration;

public class KioskConfigurationConstants
{
	public const string ConfigurationFileName = "configuration.json";

	public static string ConfigurationFilePath
	{
		get
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create);
			return Path.Combine(folderPath, "Redbox\\Configuration", "configuration.json");
		}
	}
}
