using System;
using System.Collections.Generic;
using System.IO;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.FileSets;

internal class Shared
{
	internal static void SafeDelete(string path)
	{
		List<Error> errors = new List<Error>();
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception arg)
		{
			errors.Add(new Error
			{
				Message = $"FileSetService.SafeDelete An unhandled exception occurred. Exception {arg}"
			});
		}
	}
}
