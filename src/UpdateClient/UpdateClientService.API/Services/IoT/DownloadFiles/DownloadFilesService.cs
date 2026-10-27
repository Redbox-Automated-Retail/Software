using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;
using UpdateClientService.API.Services.IoT.IoTCommand;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.IoT.DownloadFiles;

public class DownloadFilesService : IDownloadFilesService
{
	private readonly IStoreService _store;

	private readonly IPersistentDataCacheService _cache;

	private readonly IIoTCommandClient _iotCommandClient;

	private readonly ILogger<DownloadFilesService> _logger;

	private readonly IDownloadService _downloadService;

	private readonly ICommandLineService _cmdService;

	private readonly AppSettings _settings;

	private readonly TimeSpan _proxiedS3UrlRequestTimeout = TimeSpan.FromSeconds(60.0);

	public DownloadFilesService(ILogger<DownloadFilesService> logger, IStoreService store, IPersistentDataCacheService cache, IIoTCommandClient iotCommandClient, IDownloadService downloadService, ICommandLineService cmdService, IOptionsMonitor<AppSettings> settings)
	{
		_logger = logger;
		_store = store;
		_cache = cache;
		_iotCommandClient = iotCommandClient;
		_downloadService = downloadService;
		_cmdService = cmdService;
		_settings = settings.CurrentValue;
	}

	public async Task HandleDownloadFileJob(DownloadFileJob job)
	{
		DownloadFileJobExecution execution = new DownloadFileJobExecution
		{
			DownloadFileJobId = job.DownloadFileJobId,
			KioskId = _store.KioskId,
			Status = JobStatus.Scheduled
		};
		PersistentDataWrapper<DownloadFileJobList> persistentDataWrapper = await _cache.Read<DownloadFileJobList>("downloadFileList.json", useCache: true, UpdateClientServiceConstants.DownloadDataFolder);
		if (persistentDataWrapper.Data == null)
		{
			persistentDataWrapper.Data = new DownloadFileJobList();
		}
		List<DownloadFileJob> jobList = persistentDataWrapper.Data.JobList;
		if (jobList != null && jobList.Count((DownloadFileJob j) => j.DownloadFileJobId == job.DownloadFileJobId) == 0)
		{
			persistentDataWrapper.Data.JobList.Add(job);
			await _cache.Write(persistentDataWrapper.Data, "downloadFileList.json", UpdateClientServiceConstants.DownloadDataFolder);
		}
		_ = job.StartDateUtc;
		if (job.StartDateUtc == default(DateTimeOffset) || job.StartDateUtc.Add(job.StartTime) < DateTimeOffset.Now)
		{
			await ExecuteDownloadFileJob(job);
		}
		await SendDownloadFileJobExecutionStatusUpdate(execution);
	}

	private async Task<IoTCommandResponse<ObjectResult>> SendDownloadFileJobExecutionStatusUpdate(DownloadFileJobExecution execution)
	{
		return await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			RequestId = Guid.NewGuid().ToString(),
			Version = 2,
			SourceId = _store.KioskId.ToString(),
			Payload = execution,
			Command = CommandEnum.DownloadFileJobExecutionStatusUpdate
		}, new PerformIoTCommandParameters
		{
			RequestTimeout = TimeSpan.FromSeconds(15.0),
			IoTTopic = "$aws/rules/kioskrestcall",
			WaitForResponse = true
		});
	}

	public async Task ExecuteDownloadFileJob(DownloadFileJob job)
	{
		DownloadDataList downloadDataList = await _downloadService.GetDownloads(job.BitsJobId.ToString());
		if (downloadDataList.Count == 0)
		{
			await AddDownloads(job);
		}
		else if (await HandleExistingDownloads(job, downloadDataList))
		{
			await CleanupCache(job);
		}
	}

	public async Task CancelDownloadFileJob(DownloadFileJob job)
	{
		_logger.LogInfoWithSource("Canceling job -> " + job.ToJson(), "CancelDownloadFileJob", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
		DownloadFileJobExecution execution = new DownloadFileJobExecution
		{
			DownloadFileJobId = job.DownloadFileJobId,
			KioskId = _store.KioskId,
			Status = JobStatus.CancelRequested
		};
		await SendDownloadFileJobExecutionStatusUpdate(execution);
		foreach (DownloadData download in await _downloadService.GetDownloads(job.BitsJobId.ToString()))
		{
			try
			{
				await _downloadService.CancelDownload(download.Key);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "An error occurred while canceling " + download.Key, "CancelDownloadFileJob", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
			}
		}
		try
		{
			await _cache.DeleteLike(job.BitsJobId.ToString(), UpdateClientServiceConstants.DownloadDataFolder);
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, $"An error occurred while deleting jobs like {job.BitsJobId}", "CancelDownloadFileJob", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
		}
		execution.Status = JobStatus.Canceled;
		await SendDownloadFileJobExecutionStatusUpdate(execution);
	}

	public async Task<DownloadDataList> GetDownloadFileJobStatus(string bitsJobId)
	{
		return await _downloadService.GetDownloads(bitsJobId);
	}

	private async Task CleanupCache(DownloadFileJob job)
	{
		PersistentDataWrapper<DownloadFileJobList> persistentDataWrapper = await _cache.Read<DownloadFileJobList>("downloadFileList.json", useCache: false, UpdateClientServiceConstants.DownloadDataFolder);
		persistentDataWrapper.Data.JobList.RemoveAll((DownloadFileJob j) => j.DownloadFileJobId == job.DownloadFileJobId);
		await _cache.Write(persistentDataWrapper.Data, "downloadFileList.json", UpdateClientServiceConstants.DownloadDataFolder);
		await _cache.DeleteLike(job.BitsJobId.ToString(), UpdateClientServiceConstants.DownloadDataFolder);
	}

	private async Task AddDownloads(DownloadFileJob job)
	{
		DownloadFileJobExecution execution = new DownloadFileJobExecution
		{
			DownloadFileJobId = job.DownloadFileJobId,
			KioskId = _store.KioskId,
			EventTime = DateTime.Now,
			Status = JobStatus.InProgress
		};
		try
		{
			if (string.IsNullOrWhiteSpace(job.Metadata?.FileName) && string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName))
			{
				_logger.LogErrorWithSource("FileName and ActivationScriptName cannot both be null", "AddDownloads", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
				execution.CompletedOn = DateTime.Now;
				execution.Status = JobStatus.Error;
				await CleanupCache(job);
				await SendDownloadFileJobExecutionStatusUpdate(execution);
				return;
			}
			SetProxiedUrls(job);
			if (!string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName) && !string.IsNullOrWhiteSpace(job.PresignedScriptUrl))
			{
				await _downloadService.AddDownload(DownloadKey("SCRIPT", job), null, job.PresignedScriptUrl, DownloadPriority.Normal);
			}
			if (!string.IsNullOrWhiteSpace(job.Metadata?.FileName) && !string.IsNullOrWhiteSpace(job.PresignedFileUrl))
			{
				await _downloadService.AddDownload(DownloadKey("FILE", job), null, job.PresignedFileUrl, DownloadPriority.Normal);
			}
			await SendDownloadFileJobExecutionStatusUpdate(execution);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An error occurred while executing the download file job.", "AddDownloads", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
			execution.CompletedOn = DateTime.Now;
			execution.Status = JobStatus.Error;
			await SendDownloadFileJobExecutionStatusUpdate(execution);
		}
	}

	private async Task<bool> HandleExistingDownloads(DownloadFileJob job, DownloadDataList downloads)
	{
		DownloadFileJobExecution downloadFileJobExecution = new DownloadFileJobExecution
		{
			DownloadFileJobId = job.DownloadFileJobId,
			KioskId = _store.KioskId,
			EventTime = DateTime.Now,
			Status = JobStatus.InProgress
		};
		new IoTCommandModel
		{
			Command = CommandEnum.DownloadFileJobExecutionStatusUpdate,
			Payload = new MqttResponse<DownloadFileJobExecution>
			{
				Data = downloadFileJobExecution
			}
		};
		try
		{
			if (downloads.All((DownloadData d) => d.DownloadState == DownloadState.Downloading))
			{
				return false;
			}
			if (downloads.Any((DownloadData d) => d.DownloadState == DownloadState.Error))
			{
				downloadFileJobExecution.Status = JobStatus.Error;
			}
			if (downloads.All((DownloadData d) => d.DownloadState == DownloadState.PostDownload))
			{
				foreach (DownloadData download in downloads)
				{
					if (downloadFileJobExecution.Status == JobStatus.Error)
					{
						break;
					}
					if (download.Key == DownloadKey("FILE", job) && !string.IsNullOrWhiteSpace(job.Metadata?.FileName))
					{
						SaveFileToDestination(job, download);
					}
					if (download.Key == DownloadKey("SCRIPT", job) && !string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName))
					{
						DownloadFileMetadata metadata = job.Metadata;
						int num;
						if (metadata == null)
						{
							num = 0;
						}
						else
						{
							string activationScriptName = metadata.ActivationScriptName;
							num = ((((activationScriptName != null) ? new bool?(!activationScriptName.EndsWith(".ps1")) : ((bool?)null)) == true) ? 1 : 0);
						}
						if (num != 0)
						{
							_logger.LogErrorWithSource("Script " + (job.Metadata.ActivationScriptName ?? "null") + " does not have a valid PowerShell extension of '.ps1'", "HandleExistingDownloads", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
							downloadFileJobExecution.Status = JobStatus.Error;
						}
						else
						{
							downloadFileJobExecution.Status = (TryDownloadAndExecuteScript(job, download) ? downloadFileJobExecution.Status : JobStatus.Error);
						}
					}
				}
				if (downloadFileJobExecution.Status != JobStatus.Error)
				{
					downloadFileJobExecution.Status = JobStatus.Complete;
				}
				downloadFileJobExecution.CompletedOn = DateTime.Now;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An error occurred while executing the download file job.", "HandleExistingDownloads", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
			downloadFileJobExecution.CompletedOn = DateTime.Now;
			downloadFileJobExecution.Status = JobStatus.Error;
		}
		bool isComplete = downloadFileJobExecution.Status == JobStatus.Complete || downloadFileJobExecution.Status == JobStatus.Error;
		if (isComplete)
		{
			await SendDownloadFileJobExecutionStatusUpdate(downloadFileJobExecution);
		}
		return isComplete;
	}

	public async Task HandleScheduledJobs()
	{
		_ = 1;
		try
		{
			PersistentDataWrapper<DownloadFileJobList> persistentDataWrapper = await _cache.Read<DownloadFileJobList>("downloadFileList.json", useCache: false, UpdateClientServiceConstants.DownloadDataFolder);
			if (persistentDataWrapper.Data == null)
			{
				persistentDataWrapper.Data = new DownloadFileJobList();
			}
			persistentDataWrapper.Data.JobList = persistentDataWrapper.Data.JobList?.Distinct(new DownloadFileJobComparer()).ToList();
			_logger.LogInfoWithSource("Handling scheduled jobs, " + (persistentDataWrapper.Data.JobList?.ToJson() ?? "null"), "HandleScheduledJobs", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
			foreach (DownloadFileJob job in persistentDataWrapper.Data.JobList)
			{
				if (job.StartDateUtc.Add(job.StartTime) < DateTimeOffset.Now)
				{
					await ExecuteDownloadFileJob(job);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while handling schedule jobs", "HandleScheduledJobs", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
		}
	}

	private void SetProxiedUrls(DownloadFileJob job)
	{
		if (!string.IsNullOrWhiteSpace(job.Metadata?.FileName))
		{
			job.PresignedFileUrl = new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", job.FileKey, DownloadPathType.DownloadFile)).ToString();
		}
		if (!string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName))
		{
			job.PresignedScriptUrl = new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", job.FileKey, DownloadPathType.ActivationScript)).ToString();
		}
	}

	private void SaveFileToDestination(DownloadFileJob job, DownloadData download)
	{
		string destFileName = Path.Combine(job.Metadata.DestinationPath, job.Metadata.FileName);
		Directory.CreateDirectory(job.Metadata.DestinationPath);
		File.Copy(download.Path, destFileName);
	}

	private bool TryDownloadAndExecuteScript(DownloadFileJob job, DownloadData download)
	{
		string text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ps1");
		Directory.CreateDirectory(Path.GetTempPath());
		File.Copy(download.Path, text);
		bool result = !_cmdService.TryExecutePowerShellScriptFromFile(text);
		File.Delete(text);
		return result;
	}

	private string DownloadKey(string prefix, DownloadFileJob job)
	{
		return $"{prefix}.{job.BitsJobId}";
	}
}
