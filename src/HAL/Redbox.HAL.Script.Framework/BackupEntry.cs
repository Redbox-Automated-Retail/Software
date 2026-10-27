using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class BackupEntry
{
	internal readonly DateTime? CreateDate;

	internal readonly string FullPath;

	internal readonly string FileName;

	internal BackupEntry(string fullPath)
	{
		FullPath = fullPath;
		FileName = Path.GetFileName(FullPath);
		try
		{
			CreateDate = File.GetCreationTime(FullPath);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[BackupEntry] Unable to obtain creation time.", e);
			CreateDate = null;
		}
	}
}
