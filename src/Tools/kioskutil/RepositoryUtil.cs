using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;

namespace Redbox.UpdateManager.KioskUtil;

public static class RepositoryUtil
{
	private const string RepositoryLocation = "C:\\Program Files\\Redbox\\REDS\\Update Manager\\.store\\.repository";

	public const string EmptyHash = "0000000000000000000000000000000000000000";

	public static void ListRevisions(string repositoryName)
	{
		List<RevLog> revLogList = GetRevLogList(repositoryName);
		revLogList.ForEach(delegate(RevLog l)
		{
			Console.WriteLine(l.Hash);
			l.Changes.ForEach(delegate(ChangeItem ci)
			{
				Console.WriteLine("Composite:" + ci.Composite + "IsSeed:" + ci.IsSeed);
			});
		});
	}

	public static void TrimRevisions(string repositoryName)
	{
		List<RevLog> revLogList = GetRevLogList(repositoryName);
		if (revLogList.Count < 1)
		{
			Console.WriteLine("No repository revisions were found. No need to trim.");
			return;
		}
		string current = ReadCurrentRevisionInternal(repositoryName);
		if (current == "0000000000000000000000000000000000000000")
		{
			Console.WriteLine("Current revision is not set. No need to trim.");
			return;
		}
		int num = revLogList.FindIndex((RevLog r) => r.Hash == current);
		if (num == -1)
		{
			Console.WriteLine("Current revision index was not found. No need to trim.");
			return;
		}
		List<int> list = new List<int>();
		for (int num2 = 1; num2 < num; num2++)
		{
			RevLog revLog = revLogList[num2];
			bool keep = false;
			revLog.Changes.ForEach(delegate(ChangeItem ci)
			{
				if (ci.IsSeed)
				{
					keep = true;
				}
			});
			if (!keep)
			{
				list.Add(num2);
			}
		}
		Console.WriteLine($"Current Hash: {current} Index:{num}");
		if (list.Count > 0)
		{
			list.ForEach(delegate(int item)
			{
				Console.WriteLine($"Deleting index: {item} ");
			});
			GetRevLog(repositoryName).RemoveAllAt(list);
		}
	}

	private static List<RevLog> GetRevLogList(string name)
	{
		return GetRevLog(name).AsList();
	}

	private static PersistentList<RevLog> GetRevLog(string name)
	{
		string revlogPath = GetRevlogPath(name);
		return new PersistentList<RevLog>(revlogPath);
	}

	private static string ReadCurrentRevisionInternal(string name)
	{
		string currentPath = GetCurrentPath(name);
		if (!File.Exists(currentPath))
		{
			return "0000000000000000000000000000000000000000";
		}
		try
		{
			using FileStream stream = new FileStream(currentPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd().ToObject<string>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RepositoryService.ReadCurrentRevisionInternal.", e);
			return "0000000000000000000000000000000000000000";
		}
	}

	private static string GetRepositoryPath(string repositoryName)
	{
		return Path.Combine("C:\\Program Files\\Redbox\\REDS\\Update Manager\\.store\\.repository", repositoryName);
	}

	private static string GetRevlogPath(string repositoryName)
	{
		return Path.Combine(GetRepositoryPath(repositoryName), ".revlog");
	}

	private static string GetCurrentPath(string repositoryName)
	{
		return Path.Combine(GetRepositoryPath(repositoryName), ".current");
	}
}
