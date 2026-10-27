using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.FileCache;

namespace Redbox.UpdateManager.FileSets;

internal class ChangeSetFile
{
	private List<IDownloader> _downloads = new List<IDownloader>();

	private IDownloadService _downloadService;

	private IFileCacheService _fileCacheService;

	private FileSetRevisionDownloader _revisionDownloader;

	private FileSetDownloader _fileSetDownloader;

	private static object _lock = new object();

	private RevisionChangeSet _changeSet;

	private const string FileSetChangeSetExt = ".changeSet";

	private const string FileSetChangeSetPath = "changesets";

	internal long FileSetId => _changeSet.FileSetId;

	internal long RevisionId => _changeSet.RevisionId;

	internal long PatchRevisionId => _changeSet.PatchRevisionId;

	internal bool IsStaged
	{
		get
		{
			if (_changeSet.State != ChangesetState.Error)
			{
				return _changeSet.State >= ChangesetState.Staged;
			}
			return false;
		}
	}

	private IDownloadService DownloadService
	{
		get
		{
			if (_downloadService == null)
			{
				_downloadService = ServiceLocator.Instance.GetService<IDownloadService>();
			}
			return _downloadService;
		}
	}

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

	internal FileSetRevisionDownloader RevisionDownloader
	{
		get
		{
			if (_revisionDownloader == null)
			{
				_revisionDownloader = new FileSetRevisionDownloader(_downloads, _changeSet.FileSetId, _changeSet.RevisionId, _changeSet.PatchRevisionId, _changeSet.FileHash, _changeSet.DownloadUrl, _changeSet.Path, _changeSet.ContentHash, _changeSet.DownloadPriority);
			}
			return _revisionDownloader;
		}
	}

	internal FileSetDownloader FileSetDownloader
	{
		get
		{
			if (_fileSetDownloader == null)
			{
				ClientFileSetRevision revision = RevisionDownloader.GetRevision();
				if (revision != null)
				{
					_fileSetDownloader = new FileSetDownloader(revision, _downloads, _changeSet.DownloadUrl, _changeSet.DownloadPriority);
				}
			}
			return _fileSetDownloader;
		}
	}

	public ChangeSetFile()
	{
	}

	public ChangeSetFile(long fileSetId, long revisionId)
	{
		_changeSet = new RevisionChangeSet
		{
			FileSetId = fileSetId,
			RevisionId = revisionId
		};
	}

	public ChangeSetFile(RevisionChangeSet changeSet)
	{
		_changeSet = changeSet;
	}

	internal bool IsActivated()
	{
		return _changeSet.State == ChangesetState.Activated;
	}

	internal bool IsInNeedsDependency()
	{
		return _changeSet.State == ChangesetState.ActivationDependencyCheck;
	}

	internal bool IsInError()
	{
		return _changeSet.State == ChangesetState.Error;
	}

	internal static void CleanUp()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			DateTime dateTime = DateTime.Now.AddDays(-10.0);
			string changeSetDirectory = GetChangeSetDirectory();
			if (!Directory.Exists(changeSetDirectory))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.CleanUp", "Error, No directory found at " + changeSetDirectory, ""));
				return;
			}
			string[] files = Directory.GetFiles(changeSetDirectory, '*' + ".changeSet", SearchOption.TopDirectoryOnly);
			foreach (string text in files)
			{
				if (File.GetCreationTime(text) <= dateTime)
				{
					Shared.SafeDelete(text);
					LogHelper.Instance.Log($"ChangeSetManager.CleanUp - Deleting file > 60 days old: {text} ", LogEntryType.Info);
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.CleanUp", "An unhandled exception occurred.", e));
		}
	}

	internal static void CreateChangeSetDirectory()
	{
		string changeSetDirectory = GetChangeSetDirectory();
		if (!Directory.Exists(changeSetDirectory))
		{
			Directory.CreateDirectory(changeSetDirectory);
		}
	}

	internal ErrorList Delete()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				Shared.SafeDelete(GetChangeSetPath());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.CleanUp", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal ErrorList CreateFromChangeSet(ClientFileSetRevisionChangeSet set)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_changeSet = ClientToRevisionChangeSet(set);
			Save();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CreateFromChangeSet", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal ErrorList Save()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				File.WriteAllText(GetChangeSetPath(), _changeSet.ToJson());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.Save", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal static ErrorList GetAllFiles(out List<ChangeSetFile> chageSetFiles)
	{
		chageSetFiles = new List<ChangeSetFile>();
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				CreateChangeSetDirectory();
				string[] files = Directory.GetFiles(GetChangeSetDirectory(), '*' + ".changeSet", SearchOption.TopDirectoryOnly);
				LogHelper.Instance.Log($"ChangeSetManager.GetAllFiles json: {files.ToJson()} ", LogEntryType.Debug);
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						RevisionChangeSet changeSet = File.ReadAllText(text).ToObject<RevisionChangeSet>();
						chageSetFiles.Add(new ChangeSetFile(changeSet));
					}
					catch (Exception e)
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.GetAllFiles", $"Error parsing changeset file deleting {text} ", e));
						Shared.SafeDelete(text);
					}
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.GetAllFiles", "Unhandled exception occurred.", e2));
		}
		return errorList;
	}

	private void AddDownloads(List<IDownloader> downloads)
	{
		_downloads.Clear();
		downloads.ForEach(delegate(IDownloader d)
		{
			string[] array = d.DownloadData.Key.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Count() > 1 && Convert.ToInt64(array[1]) == FileSetId)
			{
				_downloads.Add(d);
			}
		});
	}

	internal ErrorList ProcessChangeSet(List<IDownloader> downloads)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			AddDownloads(downloads);
			if (_changeSet.State == ChangesetState.Received)
			{
				errorList.AddRange(CheckReceived());
			}
			if (_changeSet.State == ChangesetState.DownloadingRevision)
			{
				errorList.AddRange(CheckDownloadingRevision());
			}
			if (_changeSet.State == ChangesetState.DownloadedRevision)
			{
				errorList.AddRange(CheckDownloadedRevision());
			}
			if (_changeSet.State == ChangesetState.DownloadingFileSet)
			{
				errorList.AddRange(CheckDownloadingFileSet());
			}
			if (_changeSet.State == ChangesetState.DownloadedFileSet)
			{
				errorList.AddRange(CheckDownloadedFileSet());
			}
			if (_changeSet.State == ChangesetState.Staging)
			{
				errorList.AddRange(CheckStagingFileSet());
			}
			if (_changeSet.State == ChangesetState.Staged)
			{
				errorList.AddRange(CheckStagedFileSet());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.ProcessChangeSet", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal ErrorList ProcessActivationDependencyCheck(Dictionary<long, FileSetDependencyState> dependencyStates)
	{
		string code = "ChangeSetFile.ProcessActivationDependencyCheck";
		ErrorList errorList = new ErrorList();
		try
		{
			if (_changeSet.State == ChangesetState.ActivationDependencyCheck)
			{
				errorList.AddRange(CheckActivationDependency(dependencyStates));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList ProcessActivationPending()
	{
		string code = "ChangeSetFile.ProcessActivationPending";
		ErrorList errorList = new ErrorList();
		try
		{
			if (_changeSet.State == ChangesetState.ActivationPending)
			{
				errorList.AddRange(CheckActivationPending());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList ProcessActivationBeforeActions()
	{
		string code = "ChangeSetFile.ProcessActivationBeforeActions";
		ErrorList errorList = new ErrorList();
		try
		{
			if (_changeSet.State == ChangesetState.ActivationBeforeActions)
			{
				errorList.AddRange(CheckActivationBeforeActions());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList ProcessActivating()
	{
		string code = "ChangeSetFile.ProcessActivating";
		ErrorList errorList = new ErrorList();
		try
		{
			if (_changeSet.State == ChangesetState.Activating)
			{
				errorList.AddRange(CheckActivating());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList ProcessActivationAfterActions()
	{
		string code = "ChangeSetFile.ProcessActivationAfterActions";
		ErrorList errorList = new ErrorList();
		try
		{
			if (_changeSet.State == ChangesetState.ActivationAfterActions)
			{
				errorList.AddRange(CheckActivationAfterActions());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList ProcessActivated()
	{
		string code = "ChangeSetFile.ProcessActivated";
		ErrorList errorList = new ErrorList();
		try
		{
			_ = _changeSet.State;
			_ = 35;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", e));
		}
		return errorList;
	}

	private ErrorList CheckReceived()
	{
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.Received)
		{
			return errorList;
		}
		LogHelper.Instance.Log($"ChangeSet received fileset {_changeSet.FileSetId} revision {_changeSet.RevisionId}");
		try
		{
			if (RevisionDownloader.Exists())
			{
				SetState(ChangesetState.DownloadedRevision);
				return errorList;
			}
			if (!RevisionDownloader.Exists() && !RevisionDownloader.DownloadExists())
			{
				IDownloader downloader = RevisionDownloader.AddDownload();
				if (downloader == null)
				{
					SetError("RevisionDownloader.AddDownloader failed");
					return errorList;
				}
				_downloads.Add(downloader);
			}
			SetState(ChangesetState.DownloadingRevision);
		}
		catch (Exception e)
		{
			SetError("Unhandled exception in CheckReceived");
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in CheckReceived", e));
		}
		return errorList;
	}

	private ErrorList CheckDownloadingRevision()
	{
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.DownloadingRevision)
		{
			return errorList;
		}
		LogHelper.Instance.Log($"ChangeSet downloading fileset {_changeSet.FileSetId} revision {_changeSet.RevisionId}");
		try
		{
			if (RevisionDownloader.Exists())
			{
				SetState(ChangesetState.DownloadedRevision);
				return errorList;
			}
			if (!RevisionDownloader.DownloadExists())
			{
				SetError("CheckDownloadingRevision - Download doesn't exist when it should");
				return errorList;
			}
			if (RevisionDownloader.IsDownloadError())
			{
				SetError("CheckDownloadingRevision - Download is in an error state");
				return errorList;
			}
			if (RevisionDownloader.IsDownloadComplete())
			{
				if (RevisionDownloader.CompleteDownload())
				{
					SetState(ChangesetState.DownloadedRevision);
					return errorList;
				}
				SetError("CheckDownloadingRevision - CompleteDownload failed");
				return errorList;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in CheckReceived", e));
			SetError("Unhandled exception in CheckDownloadingRevision");
		}
		return errorList;
	}

	private ErrorList CheckDownloadedRevision()
	{
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.DownloadedRevision)
		{
			return errorList;
		}
		LogHelper.Instance.Log($"ChangeSet downloaded fileset {_changeSet.FileSetId} revision {_changeSet.RevisionId}");
		try
		{
			if (FileSetDownloader == null)
			{
				SetError("Revision does not exist");
				return errorList;
			}
			if (FileSetDownloader.IsDownloaded())
			{
				SetState(ChangesetState.DownloadedFileSet);
				return errorList;
			}
			errorList.AddRange(FileSetDownloader.DownloadFileSet());
			if (errorList.ContainsError())
			{
				SetError("An error while starting fileset downloads");
			}
			else
			{
				SetState(ChangesetState.DownloadingFileSet);
			}
			return errorList;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in CheckReceived", e));
			SetError("Unhandled exception in CheckDownloadedRevision");
			return errorList;
		}
	}

	private ErrorList CheckDownloadingFileSet()
	{
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.DownloadingFileSet)
		{
			return errorList;
		}
		LogHelper.Instance.Log($"ChangeSet CheckDownloadingFileSet fileset {_changeSet.FileSetId} revision {_changeSet.RevisionId}");
		try
		{
			if (FileSetDownloader == null)
			{
				SetError("Revision does not exist");
				return errorList;
			}
			errorList.AddRange(FileSetDownloader.CompleteDownloads());
			if (errorList.ContainsError())
			{
				SetError("Complete downloads failed");
				return errorList;
			}
			if (FileSetDownloader.IsDownloading())
			{
				return errorList;
			}
			if (FileSetDownloader.IsDownloaded())
			{
				SetState(ChangesetState.DownloadedFileSet);
				return errorList;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in CheckReceived", e));
			SetError("Unhandled exception in CheckDownloadingFileSet");
		}
		return errorList;
	}

	private ErrorList CheckDownloadedFileSet()
	{
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.DownloadedFileSet)
		{
			return errorList;
		}
		LogHelper.Instance.Log($"ChangeSet CheckDownloadedFileSet fileset {_changeSet.FileSetId} revision {_changeSet.RevisionId}");
		try
		{
			SetState(ChangesetState.Staging);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CheckDownloadedFileSet", "An unhandled exception occurred", e));
			SetError("Unhandled exception in CheckDownloadedFileSet");
		}
		return errorList;
	}

	private ErrorList CheckStagingFileSet()
	{
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.Staging)
		{
			return errorList;
		}
		LogHelper.Instance.Log($"ChangeSet CheckStagingFileSet fileset {_changeSet.FileSetId} revision {_changeSet.RevisionId}");
		try
		{
			if (FileSetDownloader == null)
			{
				SetError("Revision does not exist");
				return errorList;
			}
			FileSetTransition fileSetTransition = new FileSetTransition(FileSetDownloader.Revision);
			errorList.AddRange(fileSetTransition.Stage());
			if (errorList.ContainsError())
			{
				SetError("Staging revision failed");
				return errorList;
			}
			SetState(ChangesetState.Staged);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CheckStagingFileSet", "An unhandled exception occurred", e));
			SetError("Unhandled exception in CheckStagingFileSet");
		}
		return errorList;
	}

	private ErrorList CheckStagedFileSet()
	{
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.Staged)
		{
			return errorList;
		}
		LogHelper.Instance.Log($"ChangeSet CheckStagedFileSet fileset {_changeSet.FileSetId} revision {_changeSet.RevisionId}");
		try
		{
			SetState(ChangesetState.ActivationDependencyCheck);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CheckStagedFileSet", "An unhandled exception occurred", e));
			SetError("Unhandled exception in CheckStagedFileSet");
		}
		return errorList;
	}

	private ErrorList CheckActivationDependency(Dictionary<long, FileSetDependencyState> dependencyStates)
	{
		string text = "ChangeSetFile.CheckActivationDependencyCheck";
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.ActivationDependencyCheck)
		{
			return errorList;
		}
		Log(text, "Started");
		try
		{
			_ = FileSetDownloader.Revision.Dependencies;
			if (FileSetDownloader == null)
			{
				SetError("Revision does not exist");
				return errorList;
			}
			FileSetTransition fileSetTransition = new FileSetTransition(FileSetDownloader.Revision);
			errorList.AddRange(fileSetTransition.CheckMeetsDependency(dependencyStates, out var meetsDependencies));
			if (errorList.ContainsError())
			{
				SetError("Activating revision failed");
				return errorList;
			}
			if (meetsDependencies)
			{
				SetState(ChangesetState.ActivationPending);
				return errorList;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(text, "An unhandled exception occurred", e));
			SetError($"Unhandled exception in {text}");
		}
		return errorList;
	}

	private ErrorList CheckActivationPending()
	{
		string text = "ChangeSetFile.CheckActivationPending";
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.ActivationPending)
		{
			return errorList;
		}
		Log(text, "Started");
		try
		{
			DateTime now = DateTime.Now;
			if (_changeSet.ActiveOn > now)
			{
				Log("ChangeSetFile.CheckActivationPending", $"ActiveOn date ({_changeSet.ActiveOn}) is in the future.");
				return errorList;
			}
			if (!IsValidTime())
			{
				Log("ChangeSetFile.CheckActivationPending", $"ActivationStartTime and ActivationEndTime are not valid yet ActivateStartTime {_changeSet.ActivateStartTime} ActivateEndTime {_changeSet.ActivateEndTime}.");
				return errorList;
			}
			SetState(ChangesetState.ActivationBeforeActions);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(text, "An unhandled exception occurred", e));
			SetError($"Unhandled exception in {text}");
		}
		return errorList;
	}

	private ErrorList CheckActivationBeforeActions()
	{
		string text = "ChangeSetFile.CheckActivationBeforeActions";
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.ActivationBeforeActions)
		{
			return errorList;
		}
		Log(text, "Started");
		try
		{
			if (FileSetDownloader == null)
			{
				SetError("Revision does not exist");
				return errorList;
			}
			FileSetTransition fileSetTransition = new FileSetTransition(FileSetDownloader.Revision);
			errorList.AddRange(fileSetTransition.BeforeActivate(out var success));
			if (errorList.ContainsError())
			{
				SetError($"Unhandled exception in {text}");
				return errorList;
			}
			if (success)
			{
				SetState(ChangesetState.Activating);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(text, "An unhandled exception occurred", e));
			SetError($"Unhandled exception in {text}");
		}
		return errorList;
	}

	private ErrorList CheckActivating()
	{
		string text = "ChangeSetFile.CheckActivating";
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.Activating)
		{
			return errorList;
		}
		Log(text, "Started");
		try
		{
			if (FileSetDownloader == null)
			{
				SetError("Revision does not exist");
				return errorList;
			}
			FileSetTransition fileSetTransition = new FileSetTransition(FileSetDownloader.Revision);
			errorList.AddRange(fileSetTransition.Activate());
			if (errorList.ContainsError())
			{
				SetError("Activating revision failed");
				return errorList;
			}
			SetState(ChangesetState.ActivationAfterActions);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(text, "An unhandled exception occurred", e));
			SetError($"Unhandled exception in {text}");
		}
		return errorList;
	}

	private ErrorList CheckActivationAfterActions()
	{
		string text = "ChangeSetFile.CheckActivationAfterActions";
		ErrorList errorList = new ErrorList();
		if (_changeSet.State != ChangesetState.ActivationAfterActions)
		{
			return errorList;
		}
		Log(text, "Started");
		try
		{
			if (FileSetDownloader == null)
			{
				SetError("Revision does not exist");
				return errorList;
			}
			FileSetTransition fileSetTransition = new FileSetTransition(FileSetDownloader.Revision);
			errorList.AddRange(fileSetTransition.AfterActivate());
			errorList.ContainsError();
			SetState(ChangesetState.Activated);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(text, "An unhandled exception occurred", e));
			SetState(ChangesetState.Activated);
		}
		return errorList;
	}

	private void Log(string methodName, string message)
	{
		LogHelper.Instance.Log($"{methodName} FileSet {_changeSet.FileSetId} Revision {_changeSet.RevisionId} Message: {message}");
	}

	private void SetState(ChangesetState state)
	{
		_changeSet.State = state;
		Save();
	}

	internal bool IsValidTime()
	{
		if (string.IsNullOrEmpty(_changeSet.ActivateStartTime) || string.IsNullOrEmpty(_changeSet.ActivateEndTime))
		{
			return true;
		}
		if (!TimeSpan.TryParse(_changeSet.ActivateStartTime, out var result))
		{
			return true;
		}
		if (!TimeSpan.TryParse(_changeSet.ActivateEndTime, out var result2))
		{
			return true;
		}
		if (result == result2)
		{
			return true;
		}
		DateTime now = DateTime.Now;
		if (result2 < result)
		{
			if (!(now.TimeOfDay <= result2))
			{
				return now.TimeOfDay >= result;
			}
			return true;
		}
		if (now.TimeOfDay >= result)
		{
			return now.TimeOfDay <= result2;
		}
		return false;
	}

	private void SetError(string message)
	{
		LogHelper.Instance.Log("Setting ChangeSet Error for FileSet: {0} Revision: {1} ({2})", _changeSet.FileSetId, _changeSet.RevisionId, message);
		_changeSet.Message = message;
		_changeSet.RetryCount++;
		if (_changeSet.RetryCount > 2)
		{
			SetState(ChangesetState.Error);
		}
		else
		{
			SetState(ChangesetState.Received);
		}
	}

	private static string GetChangeSetDirectory()
	{
		return Path.Combine(FileSetService.Instance.RootPath, "changesets");
	}

	private string GetChangeSetPath()
	{
		return Path.ChangeExtension(Path.Combine(GetChangeSetDirectory(), $"{_changeSet.FileSetId}-{_changeSet.RevisionId}"), ".changeSet");
	}

	internal static RevisionChangeSet ClientToRevisionChangeSet(ClientFileSetRevisionChangeSet cs)
	{
		return new RevisionChangeSet
		{
			FileSetId = cs.FileSetId,
			RevisionId = cs.RevisionId,
			Received = DateTime.Now,
			Downloaded = null,
			Staged = null,
			Activated = null,
			State = ChangesetState.Received,
			Message = string.Empty,
			RetryCount = 0,
			Path = cs.Path,
			CompressionType = cs.CompressionType,
			ContentHash = cs.ContentHash,
			FileHash = cs.FileHash,
			PatchRevisionId = cs.PatchRevisionId,
			DownloadOn = cs.DownloadOn.ToLocalTime(),
			DownloadPriority = cs.DownloadPriority,
			ActiveOn = cs.ActiveOn.ToLocalTime(),
			ActivateStartTime = cs.ActivateStartTime,
			ActivateEndTime = cs.ActivateEndTime,
			DownloadUrl = cs.DownloadUrl
		};
	}
}
