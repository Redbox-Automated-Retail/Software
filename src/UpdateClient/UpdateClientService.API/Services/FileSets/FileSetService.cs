using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;
using UpdateClientService.API.Services.IoT.FileSets;
using UpdateClientService.API.Services.Kernel;
using UpdateClientService.API.Services.KioskEngine;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetService : IFileSetService
{
	private class RevisionChangeSetsProcessingData
	{
		public bool IsComplete { get; set; }

		public List<StateFile> InProgressStateFiles { get; set; }

		public List<RevisionChangeSet> RevisionChangeSets { get; set; }

		public DownloadDataList AllDownloadData { get; set; }

		public StateFile GetStateFileForRevisionChangeSet(RevisionChangeSet revisionChangeSet)
		{
			return InProgressStateFiles.FirstOrDefault((StateFile stateFile) => revisionChangeSet.FileSetId == stateFile.FileSetId && revisionChangeSet.RevisionId == stateFile.InProgressRevisionId);
		}
	}

	private static readonly string _root = Constants.FileSetsRoot;

	private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private static readonly TimeSpan _processLockTimeout = TimeSpan.FromSeconds(20.0);

	private static DateTime _lastCallToProcessPendingFileSetVersions = DateTime.MinValue;

	private static SemaphoreSlim _lockPendingFileSetVersions = new SemaphoreSlim(1, 1);

	private readonly int _pendingFileSetVersionsLockTimeout = 5000;

	private readonly IFileCacheService _fileCacheService;

	private readonly IDownloadService _downloadService;

	private readonly IDownloader _downloader;

	private readonly IChangeSetFileService _changeSetFileService;

	private readonly ILogger<FileSetService> _logger;

	private readonly IKioskFileSetVersionsService _versionsService;

	private readonly IKernelService _kernelService;

	private readonly IKioskEngineService _kioskEngineService;

	private readonly IStateFileService _stateFileService;

	private readonly IFileSetTransition _fileSetTransition;

	private readonly IOptionsMonitorKioskConfiguration _optionsMonitorKioskConfiguration;

	private readonly FileSetSettings _settings;

	public FileSetService(IFileCacheService fileCacheService, IDownloadService downloadService, IDownloader downloader, IChangeSetFileService changeSetFileService, ILogger<FileSetService> logger, IKioskFileSetVersionsService versionsService, IKernelService kernelService, IKioskEngineService kioskEngineService, IStateFileService stateFileService, IOptionsMonitor<AppSettings> settings, IFileSetTransition fileSetTransition, IOptionsMonitorKioskConfiguration optionsKioskConfiguration)
	{
		_fileCacheService = fileCacheService;
		_downloadService = downloadService;
		_downloader = downloader;
		_changeSetFileService = changeSetFileService;
		_logger = logger;
		_versionsService = versionsService;
		_kernelService = kernelService;
		_kioskEngineService = kioskEngineService;
		_stateFileService = stateFileService;
		_settings = settings.CurrentValue.FileSet;
		_fileSetTransition = fileSetTransition;
		_optionsMonitorKioskConfiguration = optionsKioskConfiguration;
		Initialize();
	}

	public bool Initialize()
	{
		try
		{
			Directory.CreateDirectory(_root);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while initializing FileSetService", "Initialize", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			return false;
		}
	}

	public async Task<ProcessChangeSetResponse> ProcessChangeSet(ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
	{
		_ = 2;
		try
		{
			if (clientFileSetRevisionChangeSet == null)
			{
				_logger.LogErrorWithSource("Changeset is null.", "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				return new ProcessChangeSetResponse
				{
					StatusCode = HttpStatusCode.InternalServerError
				};
			}
			_logger.LogInfoWithSource("Starting processing for " + clientFileSetRevisionChangeSet?.IdentifyingText(), "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			if (!_optionsMonitorKioskConfiguration.FileSets.FileSetDownloadEnabled)
			{
				_logger.LogWarningWithSource("FileSet downloads are disabled by config setting FileSets.FileSetDownloadEnabled.  Aborting FileSet download for " + clientFileSetRevisionChangeSet?.IdentifyingText() + " ", "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				return new ProcessChangeSetResponse
				{
					StatusCode = HttpStatusCode.ServiceUnavailable
				};
			}
			try
			{
				if (!(await _lock.WaitAsync(_processLockTimeout)))
				{
					_logger.LogWarningWithSource("Unable to aquire lock for processing " + clientFileSetRevisionChangeSet?.IdentifyingText() + ".", "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
					return new ProcessChangeSetResponse
					{
						StatusCode = HttpStatusCode.ServiceUnavailable
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while trying to aquire lock for processing " + clientFileSetRevisionChangeSet?.IdentifyingText() + ".", "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				return new ProcessChangeSetResponse
				{
					StatusCode = HttpStatusCode.InternalServerError
				};
			}
			try
			{
				if (CreateFileSetPollRequest(clientFileSetRevisionChangeSet, (await StartRevisionChangeSetProcessing(new List<ClientFileSetRevisionChangeSet> { clientFileSetRevisionChangeSet })) ? FileSetState.InProgress : FileSetState.Error, out var fileSetPollRequest))
				{
					await _versionsService.ReportFileSetVersion(fileSetPollRequest);
				}
			}
			finally
			{
				SemaphoreSlim semaphoreSlim = _lock;
				if (semaphoreSlim != null && semaphoreSlim.CurrentCount == 0)
				{
					_lock.Release();
				}
			}
		}
		catch (Exception ex2)
		{
			_logger?.LogErrorWithSource(ex2, "Exception while processing changeset " + clientFileSetRevisionChangeSet?.IdentifyingText(), "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			return new ProcessChangeSetResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
		return new ProcessChangeSetResponse
		{
			StatusCode = HttpStatusCode.OK
		};
	}

	private async Task<bool> StartRevisionChangeSetProcessing(IEnumerable<ClientFileSetRevisionChangeSet> clientFileSetRevisionChangeSets)
	{
		bool result = true;
		try
		{
			foreach (ClientFileSetRevisionChangeSet eachClientFileSetRevisionChangeSet in clientFileSetRevisionChangeSets)
			{
				if (eachClientFileSetRevisionChangeSet.Action == FileSetAction.Delete)
				{
					_logger.LogInfoWithSource("Deleting " + eachClientFileSetRevisionChangeSet?.IdentifyingText(), "StartRevisionChangeSetProcessing", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
					bool flag = result;
					if (flag)
					{
						flag = await _changeSetFileService.Delete(RevisionChangeSet.Create(eachClientFileSetRevisionChangeSet));
					}
					result = flag;
					if (!(await _stateFileService.Delete(eachClientFileSetRevisionChangeSet.FileSetId)))
					{
						result = false;
						_logger.LogErrorWithSource($"Unable to delete StateFile for FileSetId {eachClientFileSetRevisionChangeSet.FileSetId}", "StartRevisionChangeSetProcessing", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
					}
				}
				else if (eachClientFileSetRevisionChangeSet.Action == FileSetAction.Update)
				{
					bool flag2 = result;
					if (flag2)
					{
						flag2 = await StartRevisionChangeSetProcessing(eachClientFileSetRevisionChangeSet);
					}
					result = flag2;
				}
			}
		}
		catch (Exception ex)
		{
			result = false;
			_logger.LogErrorWithSource(ex, "Exception while starting RevisionChangeSet processing", "StartRevisionChangeSetProcessing", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
		return result;
	}

	private async Task<bool> StartRevisionChangeSetProcessing(ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
	{
		bool result = false;
		try
		{
			_logger.LogInfoWithSource("Starting processing for " + clientFileSetRevisionChangeSet?.IdentifyingText(), "StartRevisionChangeSetProcessing", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			if (await _changeSetFileService.CreateRevisionChangeSet(clientFileSetRevisionChangeSet))
			{
				if (await CreateStateFileFromChangeSet(clientFileSetRevisionChangeSet))
				{
					result = true;
				}
				else
				{
					_logger.LogErrorWithSource($"Unable to create StateFile for FileSetId {clientFileSetRevisionChangeSet?.FileSetId}", "StartRevisionChangeSetProcessing", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while starting processing of ClientFileSetRevisionChangeSet", "StartRevisionChangeSetProcessing", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
		return result;
	}

	private async Task<bool> CreateStateFileFromChangeSet(ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
	{
		StateFile stateFile = (await _stateFileService.Get(clientFileSetRevisionChangeSet.FileSetId))?.StateFile;
		if (stateFile == null)
		{
			stateFile = new StateFile(clientFileSetRevisionChangeSet.FileSetId, 0L, clientFileSetRevisionChangeSet.RevisionId, FileSetState.InProgress);
		}
		stateFile.InProgressRevisionId = clientFileSetRevisionChangeSet.RevisionId;
		stateFile.InProgressFileSetState = FileSetState.InProgress;
		StateFileResponse stateFileResponse = await _stateFileService.Save(stateFile);
		return stateFileResponse != null && stateFileResponse.StatusCode == HttpStatusCode.OK;
	}

	public async Task ProcessInProgressRevisionChangeSets()
	{
		_ = 12;
		try
		{
			if (!(await _lock.WaitAsync(_processLockTimeout)))
			{
				_logger.LogWarningWithSource("Unable to aquire lock.", "ProcessInProgressRevisionChangeSets", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				return;
			}
			RevisionChangeSetsProcessingData revisionChangeSetsProcessingData = new RevisionChangeSetsProcessingData();
			try
			{
				await GetInProgressStateFiles(revisionChangeSetsProcessingData);
				await GetRevisionChangeSets(revisionChangeSetsProcessingData);
				await DeleteBadRevisionChangeSets(revisionChangeSetsProcessingData);
				await ProcessChangeSets(revisionChangeSetsProcessingData);
				await ProcessNeedsDependencyStates(revisionChangeSetsProcessingData);
				await ProcessActivationPendingStates(revisionChangeSetsProcessingData);
				await ProcessBeforeActivationActions(revisionChangeSetsProcessingData);
				await ProcessActivating(revisionChangeSetsProcessingData);
				await ProcessAfterActivationActions(revisionChangeSetsProcessingData);
				await ProcessActivatedStates(revisionChangeSetsProcessingData);
				await ProcessErrorStates(revisionChangeSetsProcessingData);
				await RebootIfNeeded(revisionChangeSetsProcessingData);
				ProcessPostDownloadStates(revisionChangeSetsProcessingData);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while processing in progress RevisionChangeSets", "ProcessInProgressRevisionChangeSets", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			}
			finally
			{
				_lock.Release();
			}
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, "Exception while processing in progress RevisionChangeSets.", "ProcessInProgressRevisionChangeSets", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
	}

	private void ProcessPostDownloadStates(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		foreach (DownloadData allDownloadDatum in revisionChangeSetsProcessingData.AllDownloadData)
		{
			if (allDownloadDatum.DownloadState == DownloadState.PostDownload)
			{
				_logger.LogInfoWithSource("Completing leftover DownloadData " + allDownloadDatum.FileName, "ProcessPostDownloadStates", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				_downloader.Complete(allDownloadDatum);
			}
		}
	}

	private async Task RebootIfNeeded(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (!revisionChangeSetsProcessingData.IsComplete && _fileSetTransition.RebootRequired)
		{
			await Reboot();
		}
	}

	private async Task ProcessErrorStates(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		foreach (RevisionChangeSet eachRevisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets.Where((RevisionChangeSet x) => x.State == ChangesetState.Error))
		{
			StateFile stateFile = revisionChangeSetsProcessingData.GetStateFileForRevisionChangeSet(eachRevisionChangeSet);
			if (stateFile != null)
			{
				if (stateFile.InProgressFileSetState != FileSetState.NeedsDependency)
				{
					stateFile.InProgressFileSetState = FileSetState.Error;
					await _stateFileService.Save(stateFile);
				}
				bool flag = CreateFileSetPollRequest(eachRevisionChangeSet, stateFile.InProgressFileSetState, out var fileSetPollRequest);
				bool flag2 = flag;
				if (flag2)
				{
					flag2 = (await _versionsService.ReportFileSetVersion(fileSetPollRequest)).StatusCode == HttpStatusCode.OK;
				}
				if (flag2)
				{
					await _changeSetFileService.Delete(eachRevisionChangeSet);
					await _stateFileService.DeleteInProgress(stateFile.FileSetId);
				}
			}
		}
	}

	private async Task ProcessActivatedStates(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		foreach (RevisionChangeSet eachRevisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets.Where((RevisionChangeSet x) => x.State == ChangesetState.Activated))
		{
			StateFile stateFileForRevisionChangeSet = revisionChangeSetsProcessingData.GetStateFileForRevisionChangeSet(eachRevisionChangeSet);
			if (stateFileForRevisionChangeSet != null)
			{
				stateFileForRevisionChangeSet.InProgressFileSetState = FileSetState.Active;
				await _stateFileService.Save(stateFileForRevisionChangeSet);
			}
			await _changeSetFileService.Delete(eachRevisionChangeSet);
			if (CreateFileSetPollRequest(eachRevisionChangeSet, FileSetState.Active, out var fileSetPollRequest))
			{
				await _versionsService.ReportFileSetVersion(fileSetPollRequest);
			}
		}
	}

	private async Task ProcessAfterActivationActions(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		foreach (RevisionChangeSet revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
		{
			await _changeSetFileService.ProcessActivationAfterActions(revisionChangeSet);
		}
	}

	private async Task ProcessActivating(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		foreach (RevisionChangeSet revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
		{
			await _changeSetFileService.ProcessActivating(revisionChangeSet);
		}
	}

	private async Task ProcessBeforeActivationActions(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		foreach (RevisionChangeSet revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
		{
			await _changeSetFileService.ProcessActivationBeforeActions(revisionChangeSet);
		}
	}

	private async Task ProcessActivationPendingStates(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		foreach (RevisionChangeSet revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
		{
			await _changeSetFileService.ProcessActivationPending(revisionChangeSet);
		}
	}

	private async Task ProcessNeedsDependencyStates(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		Dictionary<long, FileSetDependencyState> dependencyStates = await GetDependencyStates();
		if (dependencyStates != null)
		{
			foreach (RevisionChangeSet revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
			{
				await _changeSetFileService.ProcessActivationDependencyCheck(dependencyStates, revisionChangeSet);
			}
		}
		foreach (RevisionChangeSet revisionChangeSet2 in revisionChangeSetsProcessingData.RevisionChangeSets)
		{
			if (revisionChangeSet2.State == ChangesetState.Error && revisionChangeSet2.Message == FileSetState.NeedsDependency.ToString())
			{
				StateFile stateFileForRevisionChangeSet = revisionChangeSetsProcessingData.GetStateFileForRevisionChangeSet(revisionChangeSet2);
				if (stateFileForRevisionChangeSet != null)
				{
					stateFileForRevisionChangeSet.InProgressFileSetState = FileSetState.NeedsDependency;
					await _stateFileService.Save(stateFileForRevisionChangeSet);
				}
			}
		}
	}

	private async Task ProcessChangeSets(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		revisionChangeSetsProcessingData.AllDownloadData = await _downloadService.GetDownloads(new Regex("^(?!(FILE|SCRIPT|downloadFileList.json)).*"));
		if (revisionChangeSetsProcessingData.RevisionChangeSets.Count() == 0 && revisionChangeSetsProcessingData.AllDownloadData.Count() == 0)
		{
			revisionChangeSetsProcessingData.IsComplete = true;
			return;
		}
		foreach (RevisionChangeSet revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
		{
			await _changeSetFileService.ProcessChangeSet(revisionChangeSetsProcessingData.AllDownloadData, revisionChangeSet);
		}
	}

	private async Task DeleteBadRevisionChangeSets(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (revisionChangeSetsProcessingData.IsComplete)
		{
			return;
		}
		List<RevisionChangeSet> list = revisionChangeSetsProcessingData.RevisionChangeSets.Where((RevisionChangeSet revisionChangeSet) => !revisionChangeSetsProcessingData.InProgressStateFiles.Any((StateFile stateFile) => stateFile.FileSetId == revisionChangeSet.FileSetId && stateFile.InProgressRevisionId == revisionChangeSet.RevisionId)).ToList();
		List<StateFile> badStateFiles = revisionChangeSetsProcessingData.InProgressStateFiles.Where((StateFile stateFile) => !revisionChangeSetsProcessingData.RevisionChangeSets.Any((RevisionChangeSet revisionChangeSet) => stateFile.FileSetId == revisionChangeSet.FileSetId && stateFile.InProgressRevisionId == revisionChangeSet.RevisionId)).ToList();
		foreach (RevisionChangeSet item in list)
		{
			revisionChangeSetsProcessingData.RevisionChangeSets.Remove(item);
			await _changeSetFileService.Delete(item);
		}
		foreach (StateFile item2 in badStateFiles)
		{
			await _stateFileService.DeleteInProgress(item2.FileSetId);
		}
	}

	private async Task GetRevisionChangeSets(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		if (!revisionChangeSetsProcessingData.IsComplete)
		{
			revisionChangeSetsProcessingData.RevisionChangeSets = await _changeSetFileService.GetAllRevisionChangeSets();
		}
	}

	private async Task GetInProgressStateFiles(RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
	{
		revisionChangeSetsProcessingData.InProgressStateFiles = (await _stateFileService.GetAllInProgress())?.StateFiles;
		if (revisionChangeSetsProcessingData.InProgressStateFiles == null)
		{
			revisionChangeSetsProcessingData.IsComplete = true;
		}
		if (revisionChangeSetsProcessingData.InProgressStateFiles.Any())
		{
			_logger.LogInfoWithSource($"Processing {revisionChangeSetsProcessingData.InProgressStateFiles.Count} in progress RevisionChangeSet", "GetInProgressStateFiles", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
	}

	private bool CreateFileSetPollRequest(RevisionChangeSetKey revisionChangeSetKey, FileSetState state, out FileSetPollRequest fileSetPollRequest)
	{
		fileSetPollRequest = null;
		try
		{
			if (revisionChangeSetKey != null && revisionChangeSetKey.FileSetId > 0 && revisionChangeSetKey.RevisionId > 0)
			{
				fileSetPollRequest = new FileSetPollRequest
				{
					FileSetId = revisionChangeSetKey.FileSetId,
					FileSetRevisionId = revisionChangeSetKey.RevisionId,
					FileSetState = state
				};
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred.", "CreateFileSetPollRequest", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
		return fileSetPollRequest != null;
	}

	private async Task<Dictionary<long, FileSetDependencyState>> GetDependencyStates()
	{
		Dictionary<long, FileSetDependencyState> dependencies = new Dictionary<long, FileSetDependencyState>();
		try
		{
			List<StateFile> stateFiles = (await _stateFileService.GetAll())?.StateFiles;
			if (stateFiles == null)
			{
				return null;
			}
			List<RevisionChangeSet> revisionChangeSets = await _changeSetFileService.GetAllRevisionChangeSets();
			foreach (StateFile eachStateFile in stateFiles)
			{
				FileSetDependencyState fileSetDependencyState = new FileSetDependencyState
				{
					FileSetId = eachStateFile.FileSetId,
					IsInProgressStaged = false
				};
				if (eachStateFile.IsRevisionDownloadInProgress)
				{
					RevisionChangeSet revisionChangeSet = revisionChangeSets.Where((RevisionChangeSet cs) => cs.FileSetId == eachStateFile.FileSetId).FirstOrDefault();
					if (revisionChangeSet != null)
					{
						ClientFileSetRevision clientFileSetRevision = await _fileCacheService.GetClientFileSetRevision(revisionChangeSet);
						if (clientFileSetRevision != null)
						{
							fileSetDependencyState.InProgressRevisionId = revisionChangeSet.RevisionId;
							fileSetDependencyState.InProgressVersion = clientFileSetRevision.RevisionVersion;
							fileSetDependencyState.IsInProgressStaged = revisionChangeSet.IsStaged;
						}
					}
				}
				if (eachStateFile.HasActiveRevision)
				{
					fileSetDependencyState.ActiveRevisionId = eachStateFile.ActiveRevisionId;
					ClientFileSetRevision clientFileSetRevision2 = await _fileCacheService.GetAnyClientFileSetRevision(eachStateFile.FileSetId, eachStateFile.ActiveRevisionId);
					if (clientFileSetRevision2 != null)
					{
						fileSetDependencyState.ActiveVersion = clientFileSetRevision2.RevisionVersion;
					}
				}
				dependencies[eachStateFile.FileSetId] = fileSetDependencyState;
			}
			return dependencies;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting FileSetDependencyStates", "GetDependencyStates", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			return null;
		}
	}

	private async Task<bool> Reboot()
	{
		_fileSetTransition.ClearRebootRequired();
		bool result = false;
		PerformShutdownResponse performShutdownResponse = await _kioskEngineService.PerformShutdown(_settings.KioskEngineShutdownTimeoutMs, _settings.KioskEngineShutdownTimeoutMs);
		if (!string.IsNullOrWhiteSpace(performShutdownResponse?.Error))
		{
			_logger.LogErrorWithSource("Unable to shutdown KioskEngine.  Error Message: " + performShutdownResponse?.Error, "Reboot", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
		else if (!_kernelService.PerformShutdown(ShutdownType.Reboot))
		{
			_logger.LogErrorWithSource("Error while trying to perform reboot.", "Reboot", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
		else
		{
			result = true;
		}
		return result;
	}

	public async Task<ReportFileSetVersionsResponse> ProcessPendingFileSetVersions()
	{
		_ = 2;
		try
		{
			await SetLastCallToProcessPendingFileSetVersions();
			ReportFileSetVersionsResponse result = await _versionsService.ReportFileSetVersions();
			if (result.StatusCode == HttpStatusCode.OK && (result.ClientFileSetRevisionChangeSets?.Any() ?? false))
			{
				foreach (ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet in result.ClientFileSetRevisionChangeSets)
				{
					await ProcessChangeSet(clientFileSetRevisionChangeSet);
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while processing pending FileSet versions", "ProcessPendingFileSetVersions", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			return new ReportFileSetVersionsResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	private async Task<DateTime> GetLastCallToProcessPendingFileSetVersion()
	{
		DateTime result = DateTime.MinValue;
		if (await _lockPendingFileSetVersions.WaitAsync(_pendingFileSetVersionsLockTimeout))
		{
			try
			{
				result = _lastCallToProcessPendingFileSetVersions;
			}
			finally
			{
				_lockPendingFileSetVersions.Release();
			}
		}
		else
		{
			_logger.LogErrorWithSource("Lock failed. Unable to get last call date for ProcessPendingFileSetVersion", "GetLastCallToProcessPendingFileSetVersion", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
		return result;
	}

	private async Task SetLastCallToProcessPendingFileSetVersions()
	{
		if (await _lockPendingFileSetVersions.WaitAsync(_pendingFileSetVersionsLockTimeout))
		{
			try
			{
				_lastCallToProcessPendingFileSetVersions = DateTime.Now;
				return;
			}
			finally
			{
				_lockPendingFileSetVersions.Release();
			}
		}
		_logger.LogErrorWithSource("Lock failed. Unable to set last call for ProcessPendingFileSetVersion", "SetLastCallToProcessPendingFileSetVersions", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
	}

	public async Task<ReportFileSetVersionsResponse> TriggerProcessPendingFileSetVersions(TriggerReportFileSetVersionsRequest triggerReportFileSetVersionsRequest)
	{
		ReportFileSetVersionsResponse response = new ReportFileSetVersionsResponse();
		try
		{
			if (triggerReportFileSetVersionsRequest != null)
			{
				if (!triggerReportFileSetVersionsRequest.ExecutionTimeFrameMs.HasValue || triggerReportFileSetVersionsRequest.ExecutionTimeFrameMs == 0)
				{
					response = await ProcessPendingFileSetVersions();
				}
				else
				{
					Task.Run(async delegate
					{
						int randomMs = new Random().Next((int)triggerReportFileSetVersionsRequest.ExecutionTimeFrameMs.Value);
						_logger.LogInfoWithSource($"waiting {randomMs} ms before calling ReportFileSetVersions", "TriggerProcessPendingFileSetVersions", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
						DateTime lastCall = await GetLastCallToProcessPendingFileSetVersion();
						await Task.Delay(randomMs);
						DateTime dateTime = lastCall;
						if (!(dateTime != await GetLastCallToProcessPendingFileSetVersion()))
						{
							await ProcessPendingFileSetVersions();
						}
					});
				}
			}
			else
			{
				_logger.LogErrorWithSource("parameter TriggerReportFileSetVersionsRequest must not be null.", "TriggerProcessPendingFileSetVersions", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				response.StatusCode = HttpStatusCode.InternalServerError;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception triggering ProcessPendingFileSetVersions.", "TriggerProcessPendingFileSetVersions", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		return response;
	}
}
