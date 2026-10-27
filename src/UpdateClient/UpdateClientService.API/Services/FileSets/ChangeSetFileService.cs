using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class ChangeSetFileService : IChangeSetFileService
{
	private readonly ILogger<ChangeSetFileService> _logger;

	private IFileSetRevisionDownloader _revisionDownloader;

	private readonly IFileSetDownloader _fileSetDownloader;

	private readonly IFileSetTransition _fileSetTransition;

	private readonly IFileCacheService _fileCacheService;

	private readonly IRevisionChangeSetRepository _revisionChangeSetRepository;

	public ChangeSetFileService(ILogger<ChangeSetFileService> logger, IFileSetRevisionDownloader revisionDownloader, IFileSetDownloader fileSetDownloader, IFileSetTransition fileSetTransition, IFileCacheService fileCacheService, IRevisionChangeSetRepository revisionChangeSetRepository)
	{
		_logger = logger;
		_revisionDownloader = revisionDownloader;
		_fileSetDownloader = fileSetDownloader;
		_fileSetTransition = fileSetTransition;
		_fileCacheService = fileCacheService;
		_revisionChangeSetRepository = revisionChangeSetRepository;
	}

	public async void CleanUp()
	{
		try
		{
			DateTime deleteDate = DateTime.Now.AddDays(-10.0);
			await _revisionChangeSetRepository.Cleanup(deleteDate);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred", "CleanUp", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		}
	}

	public async Task<bool> Delete(RevisionChangeSet revisionChangeSet)
	{
		return await _revisionChangeSetRepository.Delete(revisionChangeSet);
	}

	public async Task<bool> CreateRevisionChangeSet(ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
	{
		bool result = false;
		try
		{
			RevisionChangeSet revisionChangeSet = RevisionChangeSet.Create(clientFileSetRevisionChangeSet);
			result = await _revisionChangeSetRepository.Save(revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while creating RevisionChangeSet", "CreateRevisionChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		}
		return result;
	}

	public async Task<List<RevisionChangeSet>> GetAllRevisionChangeSets()
	{
		return await _revisionChangeSetRepository.GetAll();
	}

	public async Task ProcessChangeSet(DownloadDataList allDownloads, RevisionChangeSet revisionChangeSet)
	{
		_ = 6;
		try
		{
			DownloadDataList fileSetDownloads = allDownloads.GetByFileSetId(revisionChangeSet.FileSetId);
			await CheckReceived(revisionChangeSet, fileSetDownloads);
			await CheckDownloadingRevision(revisionChangeSet, fileSetDownloads);
			await CheckDownloadedRevision(revisionChangeSet, fileSetDownloads);
			await CheckDownloadingFileSet(revisionChangeSet, fileSetDownloads);
			await CheckDownloadedFileSet(revisionChangeSet);
			await CheckStagingFileSet(revisionChangeSet);
			await CheckStagedFileSet(revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while processing RevisionChangeSet with " + revisionChangeSet?.IdentifyingText(), "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		}
	}

	public async Task ProcessActivationDependencyCheck(Dictionary<long, FileSetDependencyState> dependencyStates, RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.ActivationDependencyCheck)
		{
			return;
		}
		LogRevisionChangeSetState(revisionChangeSet);
		try
		{
			ClientFileSetRevision clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
			if (clientFileSetRevision == null)
			{
				await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
				return;
			}
			bool flag = _fileSetTransition.CheckMeetsDependency(clientFileSetRevision, dependencyStates);
			if (!flag)
			{
				await AttemptRetryAfterError(FileSetState.NeedsDependency.ToString(), revisionChangeSet);
				return;
			}
			if (flag)
			{
				await SetState(ChangesetState.ActivationPending, revisionChangeSet);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking activation dependency for " + revisionChangeSet?.IdentifyingText(), "ProcessActivationDependencyCheck", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception", revisionChangeSet);
		}
	}

	public async Task ProcessActivationPending(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.ActivationPending)
		{
			return;
		}
		LogRevisionChangeSetState(revisionChangeSet);
		try
		{
			DateTime now = DateTime.Now;
			if (revisionChangeSet.ActiveOn > now)
			{
				_logger.LogInfoWithSource($"ActivateOn date {revisionChangeSet.ActiveOn} is in the future for {revisionChangeSet?.IdentifyingText()}", "ProcessActivationPending", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
				return;
			}
			if (!IsInActivationTime(revisionChangeSet))
			{
				_logger.LogInfoWithSource("Current time is not within Activation time window.  ActivationStartTime: " + revisionChangeSet.ActivateStartTime + ", ActivationEndTime: " + revisionChangeSet.ActivateEndTime, "ProcessActivationPending", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
				return;
			}
			await SetState(ChangesetState.ActivationBeforeActions, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking ActivationPending for " + revisionChangeSet?.IdentifyingText(), "ProcessActivationPending", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception", revisionChangeSet);
		}
	}

	public async Task ProcessActivationBeforeActions(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.ActivationBeforeActions)
		{
			return;
		}
		LogRevisionChangeSetState(revisionChangeSet);
		try
		{
			ClientFileSetRevision clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
			if (clientFileSetRevision == null)
			{
				await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
				return;
			}
			bool flag = _fileSetTransition.BeforeActivate(clientFileSetRevision);
			if (!flag)
			{
				await AttemptRetryAfterError("Error while running ActivationBeforeActions.", revisionChangeSet);
				return;
			}
			if (flag)
			{
				await SetState(ChangesetState.Activating, revisionChangeSet);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking ActivationBeforeActions for " + revisionChangeSet?.IdentifyingText(), "ProcessActivationBeforeActions", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception", revisionChangeSet);
		}
	}

	public async Task ProcessActivating(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.Activating)
		{
			return;
		}
		LogRevisionChangeSetState(revisionChangeSet);
		try
		{
			ClientFileSetRevision clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
			if (clientFileSetRevision == null)
			{
				await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
				return;
			}
			if (!_fileSetTransition.Activate(clientFileSetRevision))
			{
				await AttemptRetryAfterError("Activating revision failed", revisionChangeSet);
				return;
			}
			await SetState(ChangesetState.ActivationAfterActions, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking Activating state for " + revisionChangeSet?.IdentifyingText(), "ProcessActivating", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception.", revisionChangeSet);
		}
	}

	public async Task ProcessActivationAfterActions(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.ActivationAfterActions)
		{
			return;
		}
		LogRevisionChangeSetState(revisionChangeSet);
		try
		{
			ClientFileSetRevision clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
			if (clientFileSetRevision == null)
			{
				await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
				return;
			}
			bool flag = _fileSetTransition.AfterActivate(clientFileSetRevision);
			if (!flag)
			{
				_logger.LogWarningWithSource("Errors activating RevisionChangeSet for " + revisionChangeSet?.IdentifyingText() + ": " + flag.ToJson(), "ProcessActivationAfterActions", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			}
			await SetState(ChangesetState.Activated, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking ActivationAfterActions for " + revisionChangeSet?.IdentifyingText(), "ProcessActivationAfterActions", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await SetState(ChangesetState.Activated, revisionChangeSet);
		}
	}

	private async Task CheckReceived(RevisionChangeSet revisionChangeSet, DownloadDataList downloadDataList)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.Received)
		{
			return;
		}
		DownloadData byRevisionChangeSetKey = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
		LogRevisionChangeSetState(revisionChangeSet, byRevisionChangeSetKey);
		try
		{
			if (_revisionDownloader.DoesRevisionExist(revisionChangeSet))
			{
				await SetState(ChangesetState.DownloadedRevision, revisionChangeSet);
				return;
			}
			if (!_revisionDownloader.DoesRevisionExist(revisionChangeSet) && byRevisionChangeSetKey == null)
			{
				DownloadData downloadData = await _revisionDownloader.AddDownload(revisionChangeSet, revisionChangeSet.FileHash, revisionChangeSet.Path, revisionChangeSet.DownloadPriority);
				if (downloadData == null)
				{
					await AttemptRetryAfterError("RevisionDownloader.AddDownloader failed", revisionChangeSet);
					return;
				}
				downloadDataList.Add(downloadData);
			}
			await SetState(ChangesetState.DownloadingRevision, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking received state for " + revisionChangeSet?.IdentifyingText(), "CheckReceived", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception in CheckReceived", revisionChangeSet);
		}
	}

	private void LogRevisionChangeSetState(RevisionChangeSet revisionChangeSet, DownloadData downloadData = null)
	{
		string arg = ((downloadData != null) ? $";  DownloadState: {downloadData?.DownloadState}" : string.Empty);
		_logger.LogInfoWithSource($"RevisionChangeSet State = {revisionChangeSet?.State} for {revisionChangeSet?.IdentifyingText()}{arg}", "LogRevisionChangeSetState", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
	}

	private async Task CheckDownloadingRevision(RevisionChangeSet revisionChangeSet, DownloadDataList downloadDataList)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.DownloadingRevision)
		{
			return;
		}
		DownloadData byRevisionChangeSetKey = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
		LogRevisionChangeSetState(revisionChangeSet, byRevisionChangeSetKey);
		try
		{
			if (_revisionDownloader.DoesRevisionExist(revisionChangeSet))
			{
				await SetState(ChangesetState.DownloadedRevision, revisionChangeSet);
				return;
			}
			if (byRevisionChangeSetKey == null)
			{
				await AttemptRetryAfterError("CheckDownloadingRevision - Download doesn't exist when it should", revisionChangeSet);
				return;
			}
			if (_revisionDownloader.IsDownloadError(byRevisionChangeSetKey))
			{
				await AttemptRetryAfterError("CheckDownloadingRevision - Download is in an error state", revisionChangeSet);
				return;
			}
			if (_revisionDownloader.IsDownloadComplete(revisionChangeSet, byRevisionChangeSetKey))
			{
				if (!_revisionDownloader.CompleteDownload(revisionChangeSet, byRevisionChangeSetKey))
				{
					await AttemptRetryAfterError("CheckDownloadingRevision - CompleteDownload failed", revisionChangeSet);
				}
				else
				{
					await SetState(ChangesetState.DownloadedRevision, revisionChangeSet);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking DownloadingRevision state for " + revisionChangeSet?.IdentifyingText(), "CheckDownloadingRevision", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception in CheckDownloadingRevision", revisionChangeSet);
		}
	}

	private async Task<ClientFileSetRevision> GetClientFileSetRevision(RevisionChangeSet revisionChangeSet)
	{
		return await _fileCacheService.GetClientFileSetRevision(revisionChangeSet);
	}

	private async Task CheckDownloadedRevision(RevisionChangeSet revisionChangeSet, DownloadDataList downloadDataList)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.DownloadedRevision)
		{
			return;
		}
		DownloadData byRevisionChangeSetKey = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
		LogRevisionChangeSetState(revisionChangeSet, byRevisionChangeSetKey);
		try
		{
			ClientFileSetRevision clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
			if (clientFileSetRevision == null)
			{
				await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
				return;
			}
			if (_fileSetDownloader.IsDownloaded(clientFileSetRevision))
			{
				await SetState(ChangesetState.DownloadedFileSet, revisionChangeSet);
				return;
			}
			if (!(await _fileSetDownloader.DownloadFileSet(clientFileSetRevision, downloadDataList, revisionChangeSet.DownloadPriority)))
			{
				await AttemptRetryAfterError("An error while starting fileset downloads", revisionChangeSet);
			}
			else
			{
				await SetState(ChangesetState.DownloadingFileSet, revisionChangeSet);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking DownloadedRevsion state for " + revisionChangeSet?.IdentifyingText(), "CheckDownloadedRevision", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception in CheckDownloadedRevision", revisionChangeSet);
		}
	}

	private async Task CheckDownloadingFileSet(RevisionChangeSet revisionChangeSet, DownloadDataList downloadDataList)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.DownloadingFileSet)
		{
			return;
		}
		DownloadData byRevisionChangeSetKey = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
		LogRevisionChangeSetState(revisionChangeSet, byRevisionChangeSetKey);
		try
		{
			ClientFileSetRevision clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
			if (clientFileSetRevision == null)
			{
				await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
				return;
			}
			if (downloadDataList == null || downloadDataList.Count == 0)
			{
				await AttemptRetryAfterError("Download does not exist for FileSet", revisionChangeSet);
				return;
			}
			if (!_fileSetDownloader.CompleteDownloads(clientFileSetRevision, downloadDataList))
			{
				await AttemptRetryAfterError("Complete downloads failed", revisionChangeSet);
				return;
			}
			if (downloadDataList.IsDownloading)
			{
				return;
			}
			if (_fileSetDownloader.IsDownloaded(clientFileSetRevision))
			{
				await SetState(ChangesetState.DownloadedFileSet, revisionChangeSet);
				return;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception  while checking DownloadingFileSet state for " + revisionChangeSet?.IdentifyingText(), "CheckDownloadingFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception in CheckDownloadingFileSet", revisionChangeSet);
		}
	}

	private async Task CheckDownloadedFileSet(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet != null && revisionChangeSet.State == ChangesetState.DownloadedFileSet)
		{
			LogRevisionChangeSetState(revisionChangeSet);
			try
			{
				await SetState(ChangesetState.Staging, revisionChangeSet);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while checking DownloadedFileSet state for " + revisionChangeSet?.IdentifyingText(), "CheckDownloadedFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
				await AttemptRetryAfterError("Unhandled exception in CheckDownloadedFileSet", revisionChangeSet);
			}
		}
	}

	private async Task CheckStagingFileSet(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet == null || revisionChangeSet.State != ChangesetState.Staging)
		{
			return;
		}
		LogRevisionChangeSetState(revisionChangeSet);
		try
		{
			ClientFileSetRevision clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
			if (clientFileSetRevision == null)
			{
				await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
				return;
			}
			if (!_fileSetTransition.Stage(clientFileSetRevision))
			{
				await AttemptRetryAfterError("Staging revision failed.", revisionChangeSet);
				return;
			}
			await SetState(ChangesetState.Staged, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking Staging state for " + revisionChangeSet?.IdentifyingText(), "CheckStagingFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await AttemptRetryAfterError("Unhandled exception in CheckStagingFileSet", revisionChangeSet);
		}
	}

	private async Task CheckStagedFileSet(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet != null && revisionChangeSet.State == ChangesetState.Staged)
		{
			LogRevisionChangeSetState(revisionChangeSet);
			try
			{
				await SetState(ChangesetState.ActivationDependencyCheck, revisionChangeSet);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while checking Staged state for " + revisionChangeSet?.IdentifyingText(), "CheckStagedFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
				await AttemptRetryAfterError("Unhandled exception in CheckStagedFileSet", revisionChangeSet);
			}
		}
	}

	private async Task<bool> SetState(ChangesetState state, RevisionChangeSet revisionChangeSet)
	{
		revisionChangeSet.State = state;
		return await _revisionChangeSetRepository.Save(revisionChangeSet);
	}

	private bool IsInActivationTime(RevisionChangeSet revisionChangeSet)
	{
		if (string.IsNullOrEmpty(revisionChangeSet.ActivateStartTime) || string.IsNullOrEmpty(revisionChangeSet.ActivateEndTime))
		{
			return true;
		}
		if (!TimeSpan.TryParse(revisionChangeSet.ActivateStartTime, out var result))
		{
			return true;
		}
		if (!TimeSpan.TryParse(revisionChangeSet.ActivateEndTime, out var result2))
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

	private async Task AttemptRetryAfterError(string errorMessage, RevisionChangeSet revisionChangeSet)
	{
		_logger.LogErrorWithSource("Error for RevisionChangeSet with " + revisionChangeSet.IdentifyingText() + " (" + errorMessage + ")", "AttemptRetryAfterError", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		revisionChangeSet.Message = errorMessage;
		revisionChangeSet.RetryCount++;
		if (revisionChangeSet.RetryCount > 2)
		{
			_logger.LogInfoWithSource("Setting Error state for RevsionChangeSet with " + revisionChangeSet.IdentifyingText(), "AttemptRetryAfterError", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await SetState(ChangesetState.Error, revisionChangeSet);
		}
		else
		{
			_logger.LogInfoWithSource("Retrying download of RevisionChangeSet with " + revisionChangeSet.IdentifyingText(), "AttemptRetryAfterError", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
			await SetState(ChangesetState.Received, revisionChangeSet);
		}
	}
}
