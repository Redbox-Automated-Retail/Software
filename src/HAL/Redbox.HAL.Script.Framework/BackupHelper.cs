using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class BackupHelper
{
	private readonly string BackupPath;

	private readonly string BackupPrefix;

	internal string MakeNewBackup()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = string.Format("{0}-{1}", BackupPrefix, service.GenerateUniqueFile("xml"));
		return Path.Combine(BackupPath, path);
	}

	internal void Trim()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		int num = 5;
		List<BackupEntry> sorted = GetSorted();
		using (new DisposeableList<BackupEntry>(sorted))
		{
			if (sorted.Count > num)
			{
				for (int i = 0; i < sorted.Count - num; i++)
				{
					service.SafeDelete(sorted[i].FullPath);
				}
			}
		}
	}

	internal BackupEntry GetYoungest()
	{
		List<BackupEntry> sorted = GetSorted();
		using (new DisposeableList<BackupEntry>(sorted))
		{
			return (sorted.Count == 0) ? null : sorted[sorted.Count - 1];
		}
	}

	internal static BackupHelper GetInventoryHelper()
	{
		return new BackupHelper(ServiceLocator.Instance.GetService<IRuntimeService>().DataPath, "inventory");
	}

	internal BackupHelper(string path, string filePrefix)
	{
		BackupPath = path;
		BackupPrefix = filePrefix;
	}

	private List<BackupEntry> GetSorted()
	{
		List<BackupEntry> list = new List<BackupEntry>();
		string[] files = Directory.GetFiles(BackupPath, $"{BackupPrefix}*.xml");
		for (int i = 0; i < files.Length; i++)
		{
			BackupEntry backupEntry = new BackupEntry(files[i]);
			if (backupEntry.CreateDate.HasValue)
			{
				list.Add(backupEntry);
			}
		}
		list.Sort((BackupEntry x, BackupEntry y) => x.CreateDate.Value.CompareTo(y.CreateDate.Value));
		return list;
	}
}
