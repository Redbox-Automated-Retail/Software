using System;
using System.IO;

namespace UpdateClientService.API.Services.FileSets;

public class Constants
{
	public static string FileSetsRoot
	{
		get
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create);
			string path = Path.Combine(folderPath, "Redbox\\UpdateClient\\.store\\.filesets");
			return Path.GetFullPath(path);
		}
	}

	public static string FileCacheRootPath
	{
		get
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create);
			string path = Path.Combine(folderPath, "Redbox\\UpdateClient\\.store\\.filecache");
			return Path.GetFullPath(path);
		}
	}
}
