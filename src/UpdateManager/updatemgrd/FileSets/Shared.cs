using System;
using System.IO;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.FileSets;

internal class Shared
{
	internal static void SafeDelete(string path)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Deleting file {0}", path);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.SafeDelete", "An unhandled exception occurred.", e));
		}
	}
}
