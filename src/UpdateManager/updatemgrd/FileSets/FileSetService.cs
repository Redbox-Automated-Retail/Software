using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.FileCache;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.FileSets;

internal class FileSetService : IPollRequestReply, IFileSetService
{
	private IFileCacheService _fileCacheService;

	private bool _rebootRequired;

	private Timer _timer;

	private bool _isRunning;

	private int _inDoWork;

	private string _root;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	public static FileSetService Instance => Singleton<FileSetService>.Instance;

	public string RootPath => _root;

	private IFileCacheService FileCacheService
	{
		get
		{
			if (_fileCacheService == null)
			{
				_fileCacheService = ServiceLocator.Instance.GetService<IFileCacheService>();
			}
			return _fileCacheService;
		}
	}

	public bool RebootRequired
	{
		get
		{
			return _rebootRequired;
		}
		internal set
		{
			_rebootRequired = value;
		}
	}

	public ErrorList Initialize(string rootPath)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_root = rootPath;
			if (!Directory.Exists(_root))
			{
				Directory.CreateDirectory(_root);
			}
			LogHelper.Instance.Log("FileSet Directory: {0}", rootPath);
			ChangeSetFile.CreateChangeSetDirectory();
			_timer = new Timer(delegate
			{
				DoWork();
			});
			_isRunning = false;
			ServiceLocator.Instance.AddService(typeof(IFileSetService), this);
			LogHelper.Instance.Log("Initialized the fileset service", LogEntryType.Info);
			IHealthService service = ServiceLocator.Instance.GetService<IHealthService>();
			service.Add("FILESETSERVICE.CLEANUP", new TimeSpan(12, 0, 0), delegate
			{
				ChangeSetFile.CleanUp();
			});
			service.Add("FILESETSERVICE.FILESETCLEANUP", new TimeSpan(12, 0, 0), delegate
			{
				new FileSetCleanup().Run();
			});
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS0042", "Unhandled exception occurred in FileSetService.Initialize.", e));
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer.Change(new TimeSpan(0, 0, 35), new TimeSpan(0, 2, 0));
			_isRunning = true;
			LogHelper.Instance.Log("Starting the fileset service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS998", "An unhandled exception occurred while starting the fileset service.", e));
		}
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer.Change(-1, -1);
			_isRunning = false;
			LogHelper.Instance.Log("Stopping the fileset service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS997", "An unhandled exception occurred while stopping the fileset service.", e));
		}
		return errorList;
	}

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList errorList = new ErrorList();
		pollRequestList = new List<PollRequest>();
		try
		{
			errorList = StateFile.GetCurrentState(out var fileSetPollRequestList);
			if (errorList.Count() > 0)
			{
				return errorList;
			}
			if (fileSetPollRequestList.FileSetPollRequests.Any())
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.FileSets,
					Data = fileSetPollRequestList.ToShortFormat()
				});
			}
			LogHelper.Instance.Log("FileSetService: Store file poll request: " + pollRequestList.ToJson(), LogEntryType.Debug);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.GetPollRequest", "Unhandled exception occurred.", e));
		}
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		ErrorList errorList = new ErrorList();
		if (errorList.ContainsError() || pollReplyList == null || !pollReplyList.Any())
		{
			return errorList;
		}
		try
		{
			IEnumerable<ClientFileSetRevisionChangeSet> enumerable = pollReplyList.Select((PollReply pollReply) => pollReply.Data.ToObject<ClientFileSetRevisionChangeSet>());
			if (enumerable == null || enumerable.Count() == 0)
			{
				return errorList;
			}
			if (!_lock.TryEnterWriteLock(_lockTimeout))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.ProcessPollReply", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
				return errorList;
			}
			try
			{
				errorList.AddRange(ProcessChangeSets(enumerable));
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS0010", "An unhandled exception occurred in FileSetService.ProcessPollReply.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private FileSetService()
	{
	}

	private void DoWork()
	{
		try
		{
			if (!_isRunning)
			{
				LogHelper.Instance.Log("The fileset service is not running.", LogEntryType.Info);
				return;
			}
			if (Interlocked.CompareExchange(ref _inDoWork, 1, 0) == 1)
			{
				LogHelper.Instance.Log("Already in FileSetService.DoWork", LogEntryType.Info);
				return;
			}
			try
			{
				ErrorList errorList = new ErrorList();
				if (!_lock.TryEnterWriteLock(_lockTimeout))
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.DoWork", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
					return;
				}
				LogHelper.Instance.Log("Executing FileSetService.DoWork");
				try
				{
					ProcessChangeSetWork();
				}
				finally
				{
					_lock.ExitWriteLock();
				}
				LogHelper.Instance.Log("Finished FileSetService.DoWork");
			}
			finally
			{
				_inDoWork = 0;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an exception in FileSetService.DoWork()", e);
		}
	}

	private ErrorList ProcessChangeSets(IEnumerable<ClientFileSetRevisionChangeSet> changeSets)
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Executing FileSetService.ProcessChangeSets");
		try
		{
			foreach (ClientFileSetRevisionChangeSet changeSet in changeSets)
			{
				if (changeSet.Action == FileSetAction.Delete)
				{
					ChangeSetFile changeSetFile = new ChangeSetFile(changeSet.FileSetId, changeSet.RevisionId);
					errorList.AddRange(changeSetFile.Delete());
					StateFile stateFile = new StateFile(changeSet.FileSetId);
					errorList.AddRange(stateFile.Delete());
				}
				else if (changeSet.Action == FileSetAction.Update)
				{
					errorList.AddRange(FileSetUpdate(changeSet));
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.ProcessChangeSets", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	private ErrorList FileSetUpdate(ClientFileSetRevisionChangeSet set)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ChangeSetFile changeSetFile = new ChangeSetFile();
			errorList.AddRange(changeSetFile.CreateFromChangeSet(set));
			errorList.AddRange(StateFile.CreateFromChangeSet(set));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.FileSetUpdate", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	private void ProcessChangeSetWork()
	{
		ErrorList errors = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Executing DownloadFileService.ProcessDownloads");
			errors = StateFile.GetAllInProgress(out var list);
			if (errors.Count() > 0)
			{
				return;
			}
			errors = ChangeSetFile.GetAllFiles(out var chageSetFiles);
			if (errors.Count() > 0)
			{
				return;
			}
			var bothGood = (from state in list
				join set in chageSetFiles on new
				{
					FileSetId = state.FileSetId,
					RevisionId = state.InProgressRevisionId
				} equals new { set.FileSetId, set.RevisionId }
				select new
				{
					State = state,
					Set = set
				}).ToList();
			List<StateFile> second = bothGood.Select(item => item.State).ToList();
			List<ChangeSetFile> second2 = bothGood.Select(item => item.Set).ToList();
			list.Except(second).ToList().ForEach(delegate(StateFile item)
			{
				item.DeleteInProgress();
			});
			chageSetFiles.Except(second2).ToList().ForEach(delegate(ChangeSetFile item)
			{
				item.Delete();
			});
			IDownloadService service = ServiceLocator.Instance.GetService<IDownloadService>();
			List<IDownloader> downloads = service.GetDownloads();
			if (bothGood.Count() == 0 && downloads.Count() == 0)
			{
				return;
			}
			ServiceLocator.Instance.GetService<IKernelService>().Execute(delegate
			{
				bothGood.ForEach(item =>
				{
					errors.AddRange(item.Set.ProcessChangeSet(downloads));
				});
				Dictionary<long, FileSetDependencyState> dependencyStates = GetDependencyStates();
				bothGood.ForEach(item =>
				{
					errors.AddRange(item.Set.ProcessActivationDependencyCheck(dependencyStates));
				});
				bothGood.ForEach(item =>
				{
					if (item.Set.IsInNeedsDependency())
					{
						item.State.SetInProgressState(FileSetState.NeedsDependency);
					}
				});
				bothGood.ForEach(item =>
				{
					errors.AddRange(item.Set.ProcessActivationPending());
				});
				bothGood.ForEach(item =>
				{
					errors.AddRange(item.Set.ProcessActivationBeforeActions());
				});
				bothGood.ForEach(item =>
				{
					errors.AddRange(item.Set.ProcessActivating());
				});
				bothGood.ForEach(item =>
				{
					errors.AddRange(item.Set.ProcessActivationAfterActions());
				});
				bothGood.ForEach(item =>
				{
					errors.AddRange(item.Set.ProcessActivated());
				});
				bothGood.ForEach(item =>
				{
					if (item.Set.IsActivated())
					{
						item.State.Activate();
						item.Set.Delete();
					}
				});
				bothGood.ForEach(item =>
				{
					if (item.Set.IsInError())
					{
						item.State.SetInProgressState(FileSetState.Error);
					}
				});
				if (RebootRequired)
				{
					errors.AddRange(Reboot());
				}
				downloads.ForEach(delegate(IDownloader d)
				{
					if (d.DownloadData.DownloadState == DownloadState.PostDownload)
					{
						d.Complete();
					}
				});
			});
			LogHelper.Instance.Log("Finished executing DownloadFileService.ProcessDownloads");
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.ProcessChangeSetWork", "An unhandled exception occurred", e));
		}
	}

	private Dictionary<long, FileSetDependencyState> GetDependencyStates()
	{
		ErrorList errorList = new ErrorList();
		Dictionary<long, FileSetDependencyState> dependencies = new Dictionary<long, FileSetDependencyState>();
		try
		{
			errorList = StateFile.GetAll(out var list);
			if (errorList.Count() > 0)
			{
				return null;
			}
			errorList = ChangeSetFile.GetAllFiles(out var changeSets);
			if (errorList.Count() > 0)
			{
				return null;
			}
			list.ForEach(delegate(StateFile item)
			{
				FileSetDependencyState fileSetDependencyState = new FileSetDependencyState
				{
					FileSetId = item.FileSetId,
					IsInProgressStaged = false
				};
				if (item.HasInProgress())
				{
					ChangeSetFile changeSetFile = changeSets.Where((ChangeSetFile cs) => cs.FileSetId == item.FileSetId).FirstOrDefault();
					if (changeSetFile != null)
					{
						ClientFileSetRevision revision = GetRevision(item.FileSetId, changeSetFile.RevisionId, changeSetFile.PatchRevisionId);
						if (revision != null)
						{
							fileSetDependencyState.InProgressRevisionId = changeSetFile.RevisionId;
							fileSetDependencyState.InProgressVersion = revision.RevisionVersion;
							fileSetDependencyState.IsInProgressStaged = changeSetFile.IsStaged;
						}
					}
				}
				if (item.HasActive())
				{
					fileSetDependencyState.ActiveRevisionId = item.RevisionId;
					ClientFileSetRevision revision2 = GetRevision(item.FileSetId, item.RevisionId);
					if (revision2 != null)
					{
						fileSetDependencyState.ActiveVersion = revision2.RevisionVersion;
					}
				}
				dependencies[item.FileSetId] = fileSetDependencyState;
			});
			return dependencies;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.GetDependencyState", "An unhandled exception occurred", e));
		}
		return null;
	}

	private ClientFileSetRevision GetRevision(long fileSetId, long revisionId, long patchRevisionId)
	{
		if (FileCacheService.RevisionExists(fileSetId, revisionId, patchRevisionId) && FileCacheService.GetRevision(fileSetId, revisionId, patchRevisionId, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
		}
		return null;
	}

	public ClientFileSetRevision GetRevision(long fileSetId, long revisionId)
	{
		if (FileCacheService.GetRevision(fileSetId, revisionId, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
		}
		return null;
	}

	public ClientFileSetRevision GetRevision(string path)
	{
		if (FileCacheService.GetRevision(path, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
		}
		return null;
	}

	internal FileSetFileInfo GetFileInfo(string path)
	{
		if (FileCacheService.GetFileInfo(path, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<FileSetFileInfo>();
		}
		return null;
	}

	public void MarkRebootRequired()
	{
		_rebootRequired = true;
	}

	private ErrorList Reboot()
	{
		ErrorList errorList = new ErrorList();
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		IKioskEngineService service2 = ServiceLocator.Instance.GetService<IKioskEngineService>();
		errorList.AddRange(service2.Shutdown(15000, 2));
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
			return errorList;
		}
		service.ShutdownType = ShutdownType.Reboot;
		service.PerformShutdown();
		return errorList;
	}
}
