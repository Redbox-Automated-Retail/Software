using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Redbox.Compression;
using Redbox.Core;
using Redbox.DeltaCompression;
using Redbox.DeltaCompression.Binary;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;

namespace Redbox.UpdateManager.Environment;

public class RepositoryService : IRepositoryService, IDisposable
{
	[Flags]
	private enum MoveFileFlags
	{
		MOVEFILE_REPLACE_EXISTING = 1,
		MOVEFILE_COPY_ALLOWED = 2,
		MOVEFILE_DELAY_UNTIL_REBOOT = 4,
		MOVEFILE_WRITE_THROUGH = 8
	}

	private const int LockTimeout = 30;

	private readonly object m_dictLock = new object();

	private readonly Dictionary<string, ReaderWriterLockSlim> m_locks = new Dictionary<string, ReaderWriterLockSlim>();

	private readonly Dictionary<string, PersistentList<RevLog>> m_revLogs = new Dictionary<string, PersistentList<RevLog>>();

	public static RepositoryService Instance => Singleton<RepositoryService>.Instance;

	private string Root { get; set; }

	private string StagingDirectory { get; set; }

	public Redbox.UpdateManager.ComponentModel.ErrorList Repair(string name, out bool deferredMove)
	{
		deferredMove = false;
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
			return errorList;
		}
		try
		{
			WriteCurrentRevisionInternal(name, "0000000000000000000000000000000000000000");
			WriteActiveRevisionInternal(name, "0000000000000000000000000000000000000000");
			string headRevisionInternal = GetHeadRevisionInternal(name);
			if (headRevisionInternal == "0000000000000000000000000000000000000000")
			{
				return errorList;
			}
			errorList.AddRange(ActivateToInternal(name, headRevisionInternal, out deferredMove));
			return errorList;
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public string GetStagedRevision(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		return ReadCurrentRevisionInternal(name);
	}

	public string GetActiveRevision(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		return ReadActiveRevisionInternal(name);
	}

	public string GetActiveLabel(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		return ReadActiveLabelInternal(name);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Verify(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		List<RevLog> revLogList = GetRevLogList(name);
		for (int i = 0; i < revLogList.Count; i++)
		{
			foreach (ChangeItem change in revLogList[i].Changes)
			{
				if (change.Composite)
				{
					continue;
				}
				string text = Path.Combine(FormatRepositoryLocationInternal(name), revLogList[i].HashFileName(change.FormatFileName()));
				if (new FileInfo(text).Length < 1)
				{
					continue;
				}
				using FileStream inputStream = File.OpenRead(text);
				string text2 = inputStream.ToASCIISHA1Hash();
				if (change.IsSeed && change.VersionHash != text2)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", $"{text} should have hash {change.VersionHash} but has {text2}", "This store's repository is corrupted."));
				}
				else if (!change.IsSeed && change.ContentHash != text2)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", $"{text} should have hash {change.ContentHash} but has {text2}", "This store's repository is corrupted."));
				}
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		string current2 = ReadCurrentRevisionInternal(name);
		if (current2 != "0000000000000000000000000000000000000000")
		{
			for (int num = revLogList.FindIndex((RevLog l) => l.Hash == current2); num > -1; num--)
			{
				foreach (ChangeItem change2 in revLogList[num].Changes)
				{
					string text3 = Path.Combine(change2.TargetPath, change2.TargetName);
					if (hashSet.Contains(text3))
					{
						continue;
					}
					hashSet.Add(text3);
					string text4 = FormatStagedFileName(text3);
					using FileStream inputStream2 = File.OpenRead(text4);
					string text5 = inputStream2.ToASCIISHA1Hash();
					if (change2.VersionHash != text5)
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", $"Staged file: {text4} should have hash {change2.VersionHash} but has {text5}", "This store's repository is corrupted."));
					}
				}
			}
			hashSet.Clear();
		}
		if (ReadActiveRevisionInternal(name) != "0000000000000000000000000000000000000000")
		{
			for (int num2 = revLogList.FindIndex((RevLog l) => l.Hash == current2); num2 > -1; num2--)
			{
				foreach (ChangeItem change3 in revLogList[num2].Changes)
				{
					string text6 = Path.Combine(change3.TargetPath, change3.TargetName);
					if (hashSet.Contains(text6))
					{
						continue;
					}
					hashSet.Add(text6);
					try
					{
						using FileStream inputStream3 = File.OpenRead(text6);
						string text7 = inputStream3.ToASCIISHA1Hash();
						if (change3.VersionHash != text7)
						{
							errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", $"Active file: {text6} should have hash {change3.VersionHash} but has {text7}", "This store's repository is corrupted."));
						}
					}
					catch (Exception e)
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", $"Error hashing {text6}.", e));
					}
				}
			}
		}
		return errorList;
	}

	public List<IRevLog> GetUnfinishedChanges(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			List<IRevLog> list = new List<IRevLog>();
			ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
			IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
			Redbox.UpdateManager.ComponentModel.ErrorList jobs = service.GetJobs(out var jobs2, allUsers: false);
			if (jobs.ContainsError())
			{
				jobs.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
				{
					LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
				});
				return list;
			}
			foreach (ITransferJob item in jobs2)
			{
				ChangeSet changeSet = service2.Get<ChangeSet>(item.ID);
				if (changeSet == null || changeSet.Name != name)
				{
					continue;
				}
				Dictionary<string, IRevLog> dictionary = new Dictionary<string, IRevLog>();
				foreach (string revision in changeSet.Revisions)
				{
					RevLog revLog = new RevLog
					{
						Changes = new List<ChangeItem>(),
						Hash = revision
					};
					dictionary.Add(revision, revLog);
					list.Add(revLog);
				}
				foreach (ChangeSetItem item2 in changeSet.Items)
				{
					dictionary[item2.Revision].Changes.Add(new ChangeItem
					{
						IsSeed = item2.IsFirst,
						Composite = item2.Composite,
						ContentHash = item2.ContentHash,
						TargetName = Path.GetFileName(item2.TargetName),
						TargetPath = Path.GetDirectoryName(item2.TargetPath),
						VersionHash = item2.VersionHash
					});
				}
			}
			return list;
		}
		finally
		{
			lockForRepositoryInternal.ExitReadLock();
		}
	}

	public List<string> GetAllRepositories()
	{
		if (!Directory.Exists(Root))
		{
			Directory.CreateDirectory(Root);
		}
		string[] directories = Directory.GetDirectories(Root);
		List<string> list = new List<string>();
		DirectoryInfo directoryInfo = new DirectoryInfo(StagingDirectory);
		if (!Directory.Exists(StagingDirectory))
		{
			Directory.CreateDirectory(StagingDirectory);
		}
		string[] array = directories;
		for (int i = 0; i < array.Length; i++)
		{
			DirectoryInfo directoryInfo2 = new DirectoryInfo(array[i]);
			if (directoryInfo2.FullName != directoryInfo.FullName)
			{
				list.Add(directoryInfo2.Name);
			}
		}
		return list;
	}

	public string FormatStagedFileName(string file)
	{
		string fileName = Path.GetFileName(file);
		return Path.Combine(Path.GetDirectoryName(file), $".staged_\\{fileName}");
	}

	public void RemoveRevision(string name, string hash)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			List<int> list = new List<int>();
			PersistentList<RevLog> revLog = GetRevLog(name);
			List<RevLog> list2 = revLog.AsList();
			Predicate<RevLog> predicate2 = default(Predicate<RevLog>);
			while (true)
			{
				Predicate<RevLog> predicate = predicate2;
				if (predicate == null)
				{
					predicate = (predicate2 = (RevLog l) => l.Hash == hash);
				}
				int num = list2.FindIndex(predicate);
				if (num == -1)
				{
					break;
				}
				list.Add(num);
			}
			revLog.RemoveAllAt(list);
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public void TrimRepository(string name)
	{
		LogHelper.Instance.Log("Trimming Repository: {0}.", name);
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			List<RevLog> revLogList = GetRevLogList(name);
			if (revLogList.Count < 1)
			{
				LogHelper.Instance.Log("No repository revisions were found. No need to trim.");
				return;
			}
			string current = ReadCurrentRevisionInternal(name);
			if (current == "0000000000000000000000000000000000000000")
			{
				LogHelper.Instance.Log("Current revision is not set. No need to trim.");
				return;
			}
			int num = revLogList.FindIndex((RevLog r) => r.Hash == current);
			if (num == -1)
			{
				LogHelper.Instance.Log("Current revision index was not found. No need to trim.");
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
			LogHelper.Instance.Log("Current Hash: {0} Index:{1}", current, num);
			if (list.Count > 0)
			{
				list.ForEach(delegate(int item)
				{
					LogHelper.Instance.Log("Deleting index: {0} ", item);
				});
				GetRevLog(name).RemoveAllAt(list);
			}
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public string GetHeadRevision(string name)
	{
		if (!ContainsRepository(name))
		{
			LogHelper.Instance.Log($"repository {name} does not exist.");
			return string.Empty;
		}
		return GetHeadRevisionInternal(name);
	}

	public void UnpackChangeSet(string name, Guid id, string archive)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			LogHelper.Instance.Log("Unpacking change set.");
			ChangeSet changeSet = ServiceLocator.Instance.GetService<IDataStoreService>().Get<ChangeSet>(id);
			if ((byte)changeSet.CompressionType != 0)
			{
				using ExecutionTimer executionTimer = new ExecutionTimer();
				CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm((CompressionType)(byte)changeSet.CompressionType);
				string tempFileName = Path.GetTempFileName();
				try
				{
					using (FileStream source = File.OpenRead(archive))
					{
						using FileStream target = File.OpenWrite(tempFileName);
						algorithm.Decompress(source, target);
					}
					File.Delete(archive);
					File.Move(tempFileName, archive);
					LogHelper.Instance.Log("Decompressing change set for {0} took {1} with {2}", changeSet.Name, executionTimer.Elapsed, changeSet.CompressionType.ToString("G"));
				}
				catch (Exception)
				{
					File.Delete(tempFileName);
					throw;
				}
			}
			if (!changeSet.IsArchive)
			{
				string s = Path.Combine(changeSet.Items[changeSet.Items.Count - 1].TargetPath, changeSet.Items[changeSet.Items.Count - 1].TargetName);
				string arg = Encoding.ASCII.GetBytes(s).ToASCIISHA1Hash();
				string text = Path.Combine(StagingDirectory, $"{changeSet.Revisions.Last()}-{arg}.patch");
				File.Delete(text);
				File.Move(archive, text);
				LogHelper.Instance.Log("Unpacked file: {0}", text);
			}
			else
			{
				TarHelper.Unpack(archive, StagingDirectory);
			}
			LogHelper.Instance.Log("Finished unpacking change set.");
		}
		catch (Exception)
		{
			File.Delete(archive);
			throw;
		}
		finally
		{
			File.Delete(archive);
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public List<IRevLog> GetPendingChanges(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			string current = ReadCurrentRevisionInternal(name);
			List<RevLog> revLogList = GetRevLogList(name);
			int num = revLogList.FindIndex((RevLog l) => l.Hash == current);
			List<IRevLog> list = new List<IRevLog>();
			for (int num2 = num + 1; num2 < revLogList.Count; num2++)
			{
				list.Add(revLogList[num2]);
			}
			return list;
		}
		finally
		{
			lockForRepositoryInternal.ExitReadLock();
		}
	}

	public List<IRevLog> GetPendingActivations(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			return GetPendingActivationsInternal(name);
		}
		finally
		{
			lockForRepositoryInternal.ExitReadLock();
		}
	}

	public List<IRevLog> GetAllChanges(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			List<IRevLog> list = new List<IRevLog>();
			GetRevLogList(name).ForEach(list.Add);
			return list;
		}
		finally
		{
			lockForRepositoryInternal.ExitReadLock();
		}
	}

	public List<IRevLog> GetAppliedChanges(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			string current = ReadCurrentRevisionInternal(name);
			List<RevLog> revLogList = GetRevLogList(name);
			int num = revLogList.FindIndex((RevLog l) => l.Hash == current);
			List<IRevLog> list = new List<IRevLog>();
			for (int num2 = 0; num2 <= num; num2++)
			{
				list.Add(revLogList[num2]);
			}
			return list;
		}
		finally
		{
			lockForRepositoryInternal.ExitReadLock();
		}
	}

	public List<IRevLog> GetActivatedChanges(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			string active = ReadActiveRevisionInternal(name);
			List<RevLog> revLogList = GetRevLogList(name);
			int num = revLogList.FindIndex((RevLog l) => l.Hash == active);
			List<IRevLog> list = new List<IRevLog>();
			for (int num2 = 0; num2 <= num; num2++)
			{
				list.Add(revLogList[num2]);
			}
			return list;
		}
		finally
		{
			lockForRepositoryInternal.ExitReadLock();
		}
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RebuildTo(string name, string targetHash)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", $"repository {name} does not exists.", "Pass in a valid direcotry"));
			return errorList;
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			errorList.AddRange(RebuildToInternal(name, targetHash));
			return errorList;
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ActivateTo(string name, string targetHash, out bool deferredMove)
	{
		deferredMove = false;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", $"repository {name} does not exists.", "Pass in a valid direcotry"));
			return errorList;
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			return ActivateToInternal(name, targetHash, out deferredMove);
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList UpdateTo(string name, string targetHash)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", $"repository {name} does not exists.", "Pass in a valid direcotry"));
			return errorList;
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire write lock.");
		}
		try
		{
			return UpdateToInternal(name, targetHash, rebuilding: false);
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public bool ContainsRepository(string name)
	{
		return Directory.Exists(FormatRepositoryLocationInternal(name));
	}

	public void AddRepository(string name)
	{
		string path = FormatRepositoryLocationInternal(name);
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire write lock.");
		}
		try
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
			Directory.CreateDirectory(path);
			GetRevLog(name);
			WriteCurrentRevisionInternal(name, "0000000000000000000000000000000000000000");
			WriteActiveRevisionInternal(name, "0000000000000000000000000000000000000000");
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public void RemoveRevisions(string name, List<string> revisions)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire write lock.");
		}
		try
		{
			List<int> toRemove = new List<int>();
			PersistentList<RevLog> revLog = GetRevLog(name);
			List<RevLog> list = revLog.AsList();
			int i = 0;
			list.ForEach(delegate(RevLog item)
			{
				if (revisions.Contains(item.Hash))
				{
					toRemove.Add(i);
				}
				i++;
			});
			revLog.RemoveAllAt(toRemove);
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public void AddRevisions(string name, List<string> revisions)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire write lock.");
		}
		try
		{
			PersistentList<RevLog> revLog = GetRevLog(name);
			foreach (string revision in revisions)
			{
				revLog.Add(new RevLog
				{
					Changes = new List<ChangeItem>(),
					Hash = revision
				});
			}
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public string GetLabel(string name, string revision)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			return (GetRevLogList(name).Find((RevLog l) => l.Hash == revision) ?? throw new ApplicationException($"No revsion with hash: {revision} found in repository name: {name}")).Label;
		}
		finally
		{
			lockForRepositoryInternal.ExitReadLock();
		}
	}

	public void SetLabel(string name, string revision, string label)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			PersistentList<RevLog> revLog = GetRevLog(name);
			List<RevLog> list = revLog.AsList();
			int num = list.FindIndex((RevLog l) => l.Hash == revision);
			if (num == -1)
			{
				throw new ApplicationException($"No revsion with hash: {revision} found in repository name: {name}");
			}
			RevLog revLog2 = list[num];
			if (!string.IsNullOrEmpty(label))
			{
				revLog2.Label = label;
				revLog[num] = revLog2;
			}
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public void AddDelta(string name, List<DeltaItem> deltaItemList)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to acquire write lock.");
		}
		try
		{
			PersistentList<RevLog> revLog = GetRevLog(name);
			List<RevLog> list = revLog.AsList();
			foreach (DeltaItem deltaItem in deltaItemList)
			{
				DeltaItem item = deltaItem;
				int num = list.FindIndex((RevLog l) => l.Hash == item.Revision);
				if (num == -1)
				{
					throw new ApplicationException($"No revision with hash: {deltaItem.Revision} found in repository name: {name}");
				}
				RevLog revLog2 = list[num];
				ChangeItem changeItem = new ChangeItem
				{
					IsSeed = deltaItem.IsSeed,
					Composite = deltaItem.IsPlaceHolder,
					ContentHash = deltaItem.ContentHash,
					TargetName = deltaItem.TargetName,
					TargetPath = deltaItem.TargetPath,
					VersionHash = deltaItem.VersionHash
				};
				revLog2.Changes.Add(changeItem);
				revLog[num] = revLog2;
				string text = Path.Combine(FormatRepositoryLocationInternal(name), revLog2.HashFileName(changeItem.FormatFileName()));
				LogHelper.Instance.Log("Add patch from Revision: {0} with the following properties.\r\nComposite: {1}\r\n ContentHash: {2}\r\n TargetName: {3}\r\n TargetPath: {4}\r\n VersionHash: {5}\r\n IsSeed: {6}\r\n", deltaItem.Revision, changeItem.Composite, changeItem.ContentHash, changeItem.TargetName, changeItem.TargetPath, changeItem.VersionHash, changeItem.IsSeed);
				if (!deltaItem.IsPlaceHolder)
				{
					string arg = Encoding.ASCII.GetBytes(deltaItem.FormatFileName()).ToASCIISHA1Hash();
					if (!Directory.Exists(StagingDirectory))
					{
						Directory.CreateDirectory(StagingDirectory);
					}
					string text2 = Path.Combine(StagingDirectory, $"{deltaItem.Revision}-{arg}.patch");
					if (!File.Exists(text2))
					{
						throw new FileNotFoundException($"Could not find staged file: {text2}");
					}
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					File.Move(text2, text);
				}
			}
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public void AddDelta(string name, string revision, string file, bool isSeed, bool isPlaceHolder, string contentHash, string versionHash)
	{
		DeltaItem item = new DeltaItem
		{
			Revision = revision,
			IsSeed = isSeed,
			IsPlaceHolder = isPlaceHolder,
			ContentHash = contentHash,
			TargetName = Path.GetFileName(file),
			TargetPath = Path.GetDirectoryName(file),
			VersionHash = versionHash
		};
		AddDelta(name, new List<DeltaItem> { item });
	}

	public void Initialize(string rootPath)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		Root = service.ExpandProperties(rootPath);
		if (!Path.IsPathRooted(Root))
		{
			Root = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), rootPath);
		}
		StagingDirectory = Path.Combine(Root, "____");
		if (!Directory.Exists(Root))
		{
			Directory.CreateDirectory(Root);
		}
		if (!Directory.Exists(StagingDirectory))
		{
			Directory.CreateDirectory(StagingDirectory);
		}
		if (ServiceLocator.Instance.GetService<IRepositoryService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IRepositoryService), this);
		}
	}

	public void Dispose()
	{
		foreach (ReaderWriterLockSlim value in m_locks.Values)
		{
			try
			{
				value.Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Exception while disposeing read write lock.", e);
			}
		}
	}

	public void Reset(string name, out bool defferedDelete)
	{
		defferedDelete = false;
		List<RevLog> revLogList = GetRevLogList(name);
		HashSet<string> hashSet = new HashSet<string>();
		string current = ReadCurrentRevisionInternal(name);
		int num = revLogList.FindIndex((RevLog l) => l.Hash == current);
		if (num > -1)
		{
			for (int num2 = num; num2 < revLogList.Count; num2++)
			{
				foreach (ChangeItem change in revLogList[num2].Changes)
				{
					string text = Path.Combine(change.TargetPath, change.TargetName);
					if (!hashSet.Contains(text))
					{
						hashSet.Add(text);
						string text2 = FormatStagedFileName(text);
						try
						{
							File.Delete(text2);
						}
						catch (Exception)
						{
							DeleteLockedFileSystemEntry(text2);
							defferedDelete = true;
						}
					}
				}
			}
		}
		string path = FormatRepositoryLocationInternal(name);
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
		}
	}

	public void Reset(out bool defferedDelete)
	{
		defferedDelete = false;
		foreach (string allRepository in GetAllRepositories())
		{
			Reset(allRepository, out defferedDelete);
		}
		if (Directory.Exists(Root))
		{
			Directory.Delete(Root, recursive: true);
		}
		Initialize(Root);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RebuildToHead(string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", $"repository {name} does not exists.", "Pass in a valid direcotry"));
			return errorList;
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
			return errorList;
		}
		try
		{
			string headRevisionInternal = GetHeadRevisionInternal(name);
			if (headRevisionInternal == "0000000000000000000000000000000000000000")
			{
				return new Redbox.UpdateManager.ComponentModel.ErrorList();
			}
			errorList.AddRange(RebuildToInternal(name, headRevisionInternal));
			return errorList;
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ActivateToHead(string name, out bool deferredMove)
	{
		deferredMove = false;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", $"repository {name} does not exists.", "Pass in a valid direcotry"));
			return errorList;
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
			return errorList;
		}
		try
		{
			string headRevisionInternal = GetHeadRevisionInternal(name);
			if (headRevisionInternal == "0000000000000000000000000000000000000000")
			{
				return new Redbox.UpdateManager.ComponentModel.ErrorList();
			}
			errorList.AddRange(ActivateToInternal(name, headRevisionInternal, out deferredMove));
			return errorList;
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList UpdateToHead(string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", $"repository {name} does not exists.", "Pass in a valid direcotry"));
			return errorList;
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
			return errorList;
		}
		try
		{
			string headRevisionInternal = GetHeadRevisionInternal(name);
			if (headRevisionInternal == "0000000000000000000000000000000000000000")
			{
				return new Redbox.UpdateManager.ComponentModel.ErrorList();
			}
			errorList.AddRange(UpdateToInternal(name, headRevisionInternal, rebuilding: false));
			return errorList;
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public bool Subscribed(string name)
	{
		return !File.Exists(Path.Combine(FormatRepositoryLocationInternal(name), ".subscription"));
	}

	public void DisableSubscription(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			File.WriteAllText(Path.Combine(FormatRepositoryLocationInternal(name), ".subscription"), "0");
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	public void EndableSubscription(string name)
	{
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		ReaderWriterLockSlim lockForRepositoryInternal = GetLockForRepositoryInternal(name);
		if (!lockForRepositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
		{
			throw new TimeoutException("Unable to aquire read lock.");
		}
		try
		{
			File.Delete(Path.Combine(FormatRepositoryLocationInternal(name), ".subscription"));
		}
		finally
		{
			lockForRepositoryInternal.ExitWriteLock();
		}
	}

	private RepositoryService()
	{
	}

	[DllImport("Kernel32.dll")]
	private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

	public Redbox.UpdateManager.ComponentModel.ErrorList RebuildToInternal(string name, string targetHash)
	{
		Reset(name, out var _);
		Directory.CreateDirectory(FormatRepositoryLocationInternal(name));
		return ServiceLocator.Instance.GetService<IUpdateService>().Poll();
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList ActivateToInternal(string name, string targetHash, out bool deferredMove)
	{
		deferredMove = false;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			throw new ArgumentException($"repository {name} does not exists.", "name");
		}
		if (targetHash == "0000000000000000000000000000000000000000")
		{
			LogHelper.Instance.Log("TargetHash = {0} for name = {1} exiting.", targetHash, name);
			return errorList;
		}
		LogHelper.Instance.Log("Activating Repository: {0} to '{1}'", name, targetHash);
		string text = ReadCurrentRevisionInternal(name);
		if (text != targetHash)
		{
			errorList.AddRange(UpdateToInternal(name, targetHash, rebuilding: false));
			if (errorList.ContainsError())
			{
				LogHelper.Instance.Log("{3} errors occurred updating {0} from {1} to {2} in activate.", name, text, targetHash, errorList.Count);
				errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error error)
				{
					LogHelper.Instance.Log("{0} Details: {1}", error, error.Details);
				});
				return errorList;
			}
			text = ReadCurrentRevisionInternal(name);
		}
		try
		{
			if (ReadActiveRevisionInternal(name) == text)
			{
				return errorList;
			}
			List<IRevLog> pendingActivationsInternal = GetPendingActivationsInternal(name);
			HashSet<string> hashSet = new HashSet<string>();
			string label = string.Empty;
			foreach (IRevLog item in pendingActivationsInternal)
			{
				if (item.Changes.Count > 0)
				{
					LogHelper.Instance.Log("Activating revision {0}{1}{1}", item.Hash, System.Environment.NewLine);
				}
				foreach (ChangeItem change in item.Changes)
				{
					if (change.Composite)
					{
						continue;
					}
					string text2 = change.FormatFileName();
					if (!hashSet.Contains(text2))
					{
						LogHelper.Instance.Log("Activating file {0} to {1}{2}{2}", FormatStagedFileName(text2), text2, System.Environment.NewLine);
						hashSet.Add(text2);
						string text3 = FormatStagedFileName(text2);
						if (!File.Exists(text3))
						{
							errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", $"Error Activating staged files for repository: {name} to revision: {targetHash}", $"Staged file does not exist.File:{text3}"));
							Reset(name, out var _);
							return errorList;
						}
						try
						{
							File.Copy(FormatStagedFileName(text2), text2, overwrite: true);
						}
						catch (Exception e)
						{
							LogHelper.Instance.Log("{0} is locked by anther process doing a deferred move.", text2);
							LogHelper.Instance.Log("Error from activate copy.", e);
							string tempFileName = Path.GetTempFileName();
							File.Copy(FormatStagedFileName(text2), tempFileName, overwrite: true);
							MoveLockedFileSystemEntry(tempFileName, text2);
							deferredMove = true;
						}
					}
				}
				label = item.Label;
				if (item.Hash == targetHash)
				{
					break;
				}
			}
			WriteActiveLabelInternal(name, label);
			WriteActiveRevisionInternal(name, targetHash);
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U999", $"Error Activating staged files for repository: {name} to revision: {targetHash}", e2));
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList UpdateToInternal(string name, string targetHash, bool rebuilding)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (!ContainsRepository(name))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("A999", $"repository {name} does not exists.", "Supply a valid repository"));
			return errorList;
		}
		if (targetHash == "0000000000000000000000000000000000000000")
		{
			return errorList;
		}
		List<RevLog> revLogList = GetRevLogList(name);
		if (revLogList.Count < 1)
		{
			return errorList;
		}
		int num = revLogList.FindIndex((RevLog r) => r.Hash == targetHash);
		if (num == -1)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("A999", $"No revision found for hash: {targetHash}", "Supply a revision repository"));
			return errorList;
		}
		string current = ReadCurrentRevisionInternal(name);
		if (current == targetHash)
		{
			return errorList;
		}
		int num2 = revLogList.FindIndex((RevLog r) => r.Hash == current);
		if (num2 == -1)
		{
			LogHelper.Instance.Log("Moving to target log index {0}.", num);
		}
		else
		{
			LogHelper.Instance.Log("Moving from current log index {0} to target log index {1}.", num2, num);
		}
		if (num < num2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("A999", "Moving to earlier version is not supported.", "Supply a revision repository"));
			return errorList;
		}
		errorList.AddRange(MoveUpInternal(name, num2, num, revLogList));
		if (errorList.ContainsError() && !rebuilding)
		{
			LogHelper.Instance.Log("Error Updating {0} from {1} to {2} attempting to rebuild.", name, current, targetHash);
			if (name.Equals("dvd-data", StringComparison.CurrentCultureIgnoreCase))
			{
				LogHelper.Instance.Log("Skipped resetting dvd-data");
				return errorList;
			}
			Redbox.UpdateManager.ComponentModel.ErrorList errorList2 = RebuildToInternal(name, targetHash);
			if (errorList2.ContainsError())
			{
				errorList.AddRange(errorList2);
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("F999", "Unable to rebuild tree after update error. Tree is in an inconsistent state.", "Manual intervention is required."));
				return errorList;
			}
			errorList.Clear();
		}
		if (!errorList.ContainsError())
		{
			WriteCurrentRevisionInternal(name, targetHash);
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList MoveUpInternal(string name, int current, int target, IList<RevLog> logList)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		try
		{
			HashSet<string> hashSet = new HashSet<string>();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			for (int i = current + 1; i <= target; i++)
			{
				foreach (ChangeItem change in logList[i].Changes)
				{
					if (change.Composite)
					{
						if (i == target)
						{
							throw new InvalidOperationException("The tree can not be upated to a version that has a placeholder patch.");
						}
						continue;
					}
					string text = Path.Combine(change.TargetPath, $".staged_\\{change.TargetName}");
					string targetPath = change.TargetPath;
					string targetName = change.TargetName;
					if (dictionary2.ContainsKey(text))
					{
						continue;
					}
					hashSet.Add(text);
					if (change.IsSeed)
					{
						dictionary2[text] = i;
						continue;
					}
					if (!File.Exists(text))
					{
						dictionary2[text] = 0;
						continue;
					}
					string stagedHash;
					using (FileStream inputStream = new FileStream(text, FileMode.Open, FileAccess.ReadWrite))
					{
						stagedHash = inputStream.ToASCIISHA1Hash();
					}
					Predicate<ChangeItem> predicate2 = default(Predicate<ChangeItem>);
					for (int j = 0; j < target; j++)
					{
						List<ChangeItem> changes = logList[j].Changes;
						Predicate<ChangeItem> predicate = predicate2;
						if (predicate == null)
						{
							predicate = (predicate2 = (ChangeItem c) => !c.Composite && c.VersionHash == stagedHash && c.TargetPath == targetPath && c.TargetName == targetName);
						}
						if (changes.FindIndex(predicate) > -1)
						{
							dictionary2[text] = j + 1;
							break;
						}
					}
				}
			}
			foreach (string item in hashSet)
			{
				if (!dictionary2.ContainsKey(item))
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("P999", $"{item} can not be patched becuase a source version was not found in the revlog.", "This repository will now be redownloaded."));
					return errorList;
				}
			}
			foreach (KeyValuePair<string, int> item2 in dictionary2)
			{
				for (int num = item2.Value; num <= target; num++)
				{
					foreach (ChangeItem change2 in logList[num].Changes)
					{
						if (change2.Composite)
						{
							if (num == target)
							{
								throw new InvalidOperationException("The tree can not be upated to a version that has a placeholder patch.");
							}
							continue;
						}
						string text2 = Path.Combine(change2.TargetPath, $".staged_\\{change2.TargetName}");
						if (text2 != item2.Key)
						{
							continue;
						}
						string text3 = Path.Combine(FormatRepositoryLocationInternal(name), logList[num].HashFileName(change2.FormatFileName()));
						string path = Path.Combine(change2.TargetPath, ".staged_\\");
						if (!Directory.Exists(path))
						{
							Directory.CreateDirectory(path);
						}
						if (change2.IsSeed)
						{
							if (!dictionary.ContainsKey(text2))
							{
								dictionary[text2] = Path.GetTempFileName();
							}
							LogHelper.Instance.Log("Moving seed from {0} to {1}", text3, text2);
							File.Copy(text3, dictionary[text2], overwrite: true);
						}
						else
						{
							if (!dictionary.ContainsKey(text2))
							{
								string destFileName = (dictionary[text2] = Path.GetTempFileName());
								File.Copy(text2, destFileName, overwrite: true);
							}
							LogHelper.Instance.Log("Patching file {0} with {1}", text2, text3);
							string tempFileName2 = Path.GetTempFileName();
							File.Delete(tempFileName2);
							errorList.AddRange(ConvertFromXDeltaErrors(XDeltaHelper.Apply(dictionary[text2], text3, tempFileName2)));
							if (errorList.ContainsError())
							{
								return errorList;
							}
							File.Delete(dictionary[text2]);
							File.Move(tempFileName2, dictionary[text2]);
						}
						string text4;
						using (FileStream inputStream2 = new FileStream(dictionary[text2], FileMode.Open, FileAccess.ReadWrite))
						{
							text4 = inputStream2.ToASCIISHA1Hash();
						}
						if (text4 != change2.VersionHash)
						{
							errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("H999", $"{text2} should have SHA1 hash of {change2.VersionHash} but it has {text4}.", "Reset the repository and rebuild it."));
							return errorList;
						}
					}
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U999", "An unhandled exception occurred while updating the tree.", e));
		}
		finally
		{
			foreach (KeyValuePair<string, string> item3 in dictionary)
			{
				if (errorList.ContainsError())
				{
					File.Delete(item3.Value);
					continue;
				}
				File.Delete(item3.Key);
				File.Move(item3.Value, item3.Key);
			}
		}
		return errorList;
	}

	private List<RevLog> GetRevLogList(string name)
	{
		return GetRevLog(name).AsList();
	}

	private PersistentList<RevLog> GetRevLog(string name)
	{
		if (!m_revLogs.ContainsKey(name))
		{
			string path = Path.Combine(FormatRepositoryLocationInternal(name), ".revlog");
			m_revLogs[name] = new PersistentList<RevLog>(path);
		}
		return m_revLogs[name];
	}

	private string ReadCurrentRevisionInternal(string name)
	{
		string path = Path.Combine(FormatRepositoryLocationInternal(name), ".current");
		if (!File.Exists(path))
		{
			return "0000000000000000000000000000000000000000";
		}
		try
		{
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd().ToObject<string>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RepositoryService.ReadCurrentRevisionInternal.", e);
			return "0000000000000000000000000000000000000000";
		}
	}

	private string ReadActiveRevisionInternal(string name)
	{
		string path = Path.Combine(FormatRepositoryLocationInternal(name), ".active");
		if (!File.Exists(path))
		{
			return "0000000000000000000000000000000000000000";
		}
		try
		{
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd().ToObject<string>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RepositoryService.ReadActiveRevisionInternal.", e);
			return "0000000000000000000000000000000000000000";
		}
	}

	private void WriteCurrentRevisionInternal(string name, string revision)
	{
		File.WriteAllText(Path.Combine(FormatRepositoryLocationInternal(name), ".current"), revision.ToJson());
	}

	private void WriteActiveRevisionInternal(string name, string revision)
	{
		File.WriteAllText(Path.Combine(FormatRepositoryLocationInternal(name), ".active"), revision.ToJson());
	}

	private string ReadActiveLabelInternal(string name)
	{
		string path = Path.Combine(FormatRepositoryLocationInternal(name), ".label");
		if (!File.Exists(path))
		{
			return string.Empty;
		}
		try
		{
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd().ToObject<string>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in RepositoryService.ReadActiveLabelInternal.", e);
			return string.Empty;
		}
	}

	private void WriteActiveLabelInternal(string name, string label)
	{
		File.WriteAllText(Path.Combine(FormatRepositoryLocationInternal(name), ".label"), label.ToJson());
	}

	private string FormatRepositoryLocationInternal(string name)
	{
		return $"{Root}\\{name}";
	}

	private ReaderWriterLockSlim GetLockForRepositoryInternal(string name)
	{
		lock (m_dictLock)
		{
			if (m_locks.ContainsKey(name))
			{
				return m_locks[name];
			}
			ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			m_locks.Add(name, readerWriterLockSlim);
			return readerWriterLockSlim;
		}
	}

	private string GetHeadRevisionInternal(string name)
	{
		List<RevLog> revLogList = GetRevLogList(name);
		if (revLogList == null || revLogList.Count < 1)
		{
			return "0000000000000000000000000000000000000000";
		}
		return revLogList[revLogList.Count - 1].Hash;
	}

	private List<IRevLog> GetPendingActivationsInternal(string name)
	{
		string activeRevision = ReadActiveRevisionInternal(name);
		List<RevLog> revLogList = GetRevLogList(name);
		int num = revLogList.FindIndex((RevLog l) => l.Hash == activeRevision);
		List<IRevLog> list = new List<IRevLog>();
		for (int num2 = num + 1; num2 < revLogList.Count; num2++)
		{
			list.Add(revLogList[num2]);
		}
		return list;
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromXDeltaErrors(List<Redbox.DeltaCompression.Error> xDeltaErrors)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
		xDeltaErrors.ForEach(delegate(Redbox.DeltaCompression.Error e)
		{
			errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
		});
		return errors;
	}

	private static void MoveLockedFileSystemEntry(string source, string destination)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(source);
		string text2 = service.ExpandProperties(destination);
		MoveFileFlags moveFileFlags = MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
		if (!Directory.Exists(text) && !Directory.Exists(text2))
		{
			moveFileFlags |= MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
		}
		MoveFileEx(text, text2, moveFileFlags);
	}

	public void DeleteLockedFileSystemEntry(string target)
	{
		MoveFileEx(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(target), null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
	}
}
