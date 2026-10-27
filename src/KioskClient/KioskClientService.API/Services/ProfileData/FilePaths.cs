using System;
using System.Collections.Generic;
using System.IO;

namespace KioskClientService.API.Services.ProfileData;

public static class FilePaths
{
	public static Dictionary<FileTypeEnum, string> TypeMappings = new Dictionary<FileTypeEnum, string> { 
	{
		FileTypeEnum.ProfileData,
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Redbox\\Data\\Profile")
	} };
}
