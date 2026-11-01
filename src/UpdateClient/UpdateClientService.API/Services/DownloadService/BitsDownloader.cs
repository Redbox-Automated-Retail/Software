using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.Transfer;

namespace UpdateClientService.API.Services.DownloadService;

public class BitsDownloader : BaseDownloader
{
	private class ProcessDownloadDataInfo
	{
		public bool Success { get; set; } = true;

		public DownloadData DownloadData { get; set; }

		public BitsDownloader BitsDownloader { get; set; }

		public async Task<bool> Update(DownloadState? downloadState = null, string errorMessage = null)
		{
			Success &= string.IsNullOrEmpty(errorMessage);
			bool updated = await BitsDownloader.UpdateDownloadData(DownloadData, downloadState, errorMessage);
			Success &= updated;
			return updated;
		}
	}

	private readonly ILogger<BitsDownloader> _logger;

	private readonly ITransferService _transferService;

	private const string DownloadTypeName = "Bits downloader";

	private const string BitsNameFormat = "<UCS::~{0}~::UCS>";

	private const string BitsNameLeft = "<UCS::~";

	private const string BitsNameRight = "~::UCS>";

	public BitsDownloader(ILogger<BitsDownloader> logger, ITransferService transferService, IPersistentDataCacheService cache)
		: base(cache, logger)
	{
		_logger = logger;
		_transferService = transferService;
	}

	public override async Task<bool> ProcessDownload(DownloadData downloadData)
	{
		ProcessDownloadDataInfo processDownloadDataInfo = new ProcessDownloadDataInfo
		{
			BitsDownloader = this,
			DownloadData = downloadData
		};
		SetBitTransferStatus(processDownloadDataInfo.DownloadData);
		await CheckNone(processDownloadDataInfo);
		await CheckDownloading(processDownloadDataInfo);
		await CheckPostDownload(processDownloadDataInfo);
		await CheckComplete(processDownloadDataInfo);
		await CheckError(processDownloadDataInfo);
		return processDownloadDataInfo.Success;
	}

	public override bool Cleanup(DownloadDataList downloadList)
	{
		bool success = true;
		try
		{
			if (!GetDownloadFileBitsJobs(out var jobs))
			{
				return false;
			}
			List<Error> errors = new List<Error>();
			foreach (ITransferJob item in jobs)
			{
				DownloadData byBitsGuid = downloadList.GetByBitsGuid(item.ID);
				if (byBitsGuid == null)
				{
					_logger.LogInfoWithSource($"Clearing bits job, guid {item.ID} that has no download file changeset.", "Cleanup", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
					errors.AddRange(item.Cancel());
				}
			}
			success = !errors.Any();
		}
		catch (Exception ex)
		{
			success = false;
			_logger.LogErrorWithSource(ex, "Exception while cleaning up DownloadDataList", "Cleanup", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
		return success;
	}

	private void SetBitTransferStatus(DownloadData downloadData)
	{
		if (downloadData.BitsGuid == default(Guid) || downloadData.DownloadState == DownloadState.Error)
		{
			return;
		}
		if (downloadData.DownloadState == DownloadState.PostDownload)
		{
			downloadData.BitsTransferred = downloadData.BitsTotal;
			return;
		}
		_transferService.GetJob(downloadData.BitsGuid, out var job);
		if (job == null)
		{
			_logger.LogInfoWithSource("Job was null for " + downloadData.ToJson(), "SetBitTransferStatus", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			return;
		}
		try
		{
			downloadData.BitsTotal = ((job.TotalBytes == ulong.MaxValue) ? long.MaxValue : Convert.ToInt64(job.TotalBytes));
			downloadData.BitsTransferred = Convert.ToInt64(job.TotalBytesTransfered);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Something went wrong while updating transfer status.", "SetBitTransferStatus", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
	}

	private TransferJobPriority GetJobPriority(DownloadData downloadData)
	{
		return downloadData.DownloadPriority switch
		{
			DownloadPriority.Normal => TransferJobPriority.Normal, 
			DownloadPriority.High => TransferJobPriority.High, 
			DownloadPriority.Foreground => TransferJobPriority.Foreground, 
			_ => TransferJobPriority.Low, 
		};
	}

	private async Task CheckNone(ProcessDownloadDataInfo processDownloadDataInfo)
	{
		DownloadData downloadData = processDownloadDataInfo.DownloadData;
		if (downloadData.DownloadState != DownloadState.None)
		{
			return;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState, downloadData.Key), "CheckNone", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		try
		{
			List<Error> errors = new List<Error>();
			string name = $"<UCS::~{downloadData.Key}~::UCS>";
			errors.AddRange(_transferService.CreateDownloadJob(name, out var job));
			errors.AddRange(job.SetMinimumRetryDelay(60u));
			errors.AddRange(job.SetNoProgressTimeout(0u));
			errors.AddRange(job.SetPriority(GetJobPriority(downloadData)));
			errors.AddRange(job.AddItem(downloadData.Url, Path.GetTempFileName()));
			if (errors.Any())
			{
				processDownloadDataInfo.Success = false;
				errors.AddRange(job.Cancel());
				if (downloadData.RetryCount == 10)
				{
					downloadData.DownloadState = DownloadState.Error;
					errors.Add(new Error
					{
						Message = $"Retry limit exceeded for {job.ID}"
					});
				}
				else
				{
					downloadData.DownloadState = DownloadState.None;
				}
				downloadData.Message = string.Join(",", from e in errors
					where !string.IsNullOrWhiteSpace(e.Message)
					select e.Message);
				downloadData.RetryCount++;
				await SaveDownload(downloadData);
			}
			else
			{
				errors.AddRange(job.Resume());
				if (errors.Any())
				{
					await processDownloadDataInfo.Update(DownloadState.Error, errors[0].Message);
					return;
				}
				downloadData.BitsGuid = job.ID;
				await processDownloadDataInfo.Update(DownloadState.Downloading);
				_logger.LogInfoWithSource($"Download Name: {downloadData.Key} has been put on the download queue. BITS job started with name: {name} and guid: {job.ID}.", "CheckNone", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			}
		}
		catch (Exception ex)
		{
			processDownloadDataInfo.Success = false;
			_logger.LogErrorWithSource(ex, "Exception while checking status None for DownloadData " + processDownloadDataInfo?.DownloadData?.ToJson(), "CheckNone", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
	}

	private async Task<bool> UpdateDownloadData(DownloadData downloadData, DownloadState? downloadState, string errorMessage = null)
	{
		if (downloadState.HasValue)
		{
			_logger.LogInfoWithSource($"Setting DownloadState = {downloadState} for DownloadData {downloadData.FileName}", "UpdateDownloadData", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			downloadData.DownloadState = downloadState.Value;
		}
		if (errorMessage != null)
		{
			downloadData.Message = errorMessage;
		}
		return await SaveDownload(downloadData);
	}

	private async Task CheckDownloading(ProcessDownloadDataInfo processDownloadDataInfo)
	{
		DownloadData downloadData = processDownloadDataInfo.DownloadData;
		if (downloadData.DownloadState != DownloadState.Downloading)
		{
			return;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState, downloadData.Key), "CheckDownloading", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		try
		{
			if (!GetDownloadFileBitsJobs(out var jobs))
			{
				await processDownloadDataInfo.Update(DownloadState.Error, "Error getting Jobs list");
				return;
			}
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == downloadData.BitsGuid).FirstOrDefault();
			if (transferJob == null)
			{
				await processDownloadDataInfo.Update(DownloadState.Error, "Bits job missing for key: " + downloadData.Key);
				return;
			}
			if (transferJob.Status == TransferStatus.Error)
			{
				transferJob.Cancel();
				await processDownloadDataInfo.Update(DownloadState.Error, "Bits job error for key: " + downloadData.Key);
				return;
			}
			if (transferJob.Status == TransferStatus.Suspended)
			{
				transferJob.Cancel();
				await processDownloadDataInfo.Update(DownloadState.Error, "Bits job suspended for key: " + downloadData.Key);
				return;
			}
			if (transferJob.Status == TransferStatus.TransientError)
			{
				_logger.LogInfoWithSource("BITS job status is TransientError for key: " + downloadData.Key, "CheckDownloading", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
				processDownloadDataInfo.Success = false;
			}
			if (transferJob.Status == TransferStatus.Transferred)
			{
				await FinishBitsDownload(transferJob, processDownloadDataInfo);
			}
		}
		catch (Exception ex)
		{
			processDownloadDataInfo.Success = false;
			_logger.LogErrorWithSource(ex, $"Exception while checking status {DownloadState.Downloading} for DownloadData {processDownloadDataInfo?.DownloadData?.ToJson()}", "CheckDownloading", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
	}

	private async Task FinishBitsDownload(ITransferJob job, ProcessDownloadDataInfo processDownloadDataInfo)
	{
		DownloadData downloadData = processDownloadDataInfo.DownloadData;
		try
		{
			_logger.LogInfoWithSource("Bits downloader FinishBitsDownload for " + downloadData.Key, "FinishBitsDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			List<Error> errors = new List<Error>();
			errors.AddRange(job.GetItems(out var items));
			errors.AddRange(job.Complete());
			if (errors.Any())
			{
				job.Cancel();
				await processDownloadDataInfo.Update(DownloadState.Error, $"BitsDownloader -> Job {job.ID} get items or completion failed. This download will be canceled.");
				return;
			}
			ITransferItem transferItem = items[0];
			string downloadPath = Path.Combine(transferItem.Path, transferItem.Name);
			bool hashValid = CheckHash(downloadPath, downloadData.Hash, out var fileHash);
			if (!string.IsNullOrWhiteSpace(downloadData.Hash) && !hashValid)
			{
				await processDownloadDataInfo.Update(DownloadState.Error, "BitsDownloader -> Download file: " + downloadData.Key + " path: " + downloadPath + " should have hash " + downloadData.Hash + " but it has hash " + fileHash + ". This download failed and will need to be restarted.");
				File.Delete(downloadPath);
			}
			else
			{
				downloadData.Path = downloadPath;
				await processDownloadDataInfo.Update(DownloadState.PostDownload);
				if (string.IsNullOrWhiteSpace(downloadData.Hash))
				{
					_logger.LogInfoWithSource("Download file: " + downloadData.Key + " was not provided with a hash to compare against. Ignoring check.", "FinishBitsDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
				}
			}
		}
		catch (Exception ex)
		{
			processDownloadDataInfo.Success = false;
			_logger.LogErrorWithSource(ex, "Exception while finishing Bits download for " + downloadData.Key, "FinishBitsDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
	}

	private async Task CheckPostDownload(ProcessDownloadDataInfo processDownloadDataInfo)
	{
		DownloadData downloadData = processDownloadDataInfo.DownloadData;
		if (downloadData.DownloadState == DownloadState.PostDownload)
		{
			_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState, downloadData.Key), "CheckPostDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			if (downloadData.CompleteOnFinish)
			{
				_logger.LogInfoWithSource("CompleteOnFinish for " + downloadData.FileName, "CheckPostDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
				bool success = processDownloadDataInfo.Success;
				processDownloadDataInfo.Success = success & await Complete(downloadData);
			}
		}
	}

	private async Task CheckComplete(ProcessDownloadDataInfo processDownloadDataInfo)
	{
		DownloadData downloadData = processDownloadDataInfo.DownloadData;
		if (downloadData.DownloadState == DownloadState.Complete)
		{
			_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState, downloadData.Key), "CheckComplete", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			bool success = processDownloadDataInfo.Success;
			processDownloadDataInfo.Success = success & await DeleteDownload(downloadData);
		}
	}

	private async Task CheckError(ProcessDownloadDataInfo processDownloadDataInfo)
	{
		DownloadData downloadData = processDownloadDataInfo.DownloadData;
		if (downloadData.DownloadState != DownloadState.Error)
		{
			return;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState, downloadData.Key), "CheckError", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		_ = downloadData.BitsGuid;
		List<Error> errors = new List<Error>();
		if (GetDownloadFileBitsJobs(out var jobs))
		{
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == downloadData.BitsGuid).FirstOrDefault();
			if (transferJob != null)
			{
				errors.AddRange(transferJob.GetErrors());
				string text = string.Join(", ", errors.Select((Error x) => x.Message));
				_logger.LogWarningWithSource("Bits job error message for DownloadData " + downloadData.Key + ": " + text, "CheckError", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
				errors.AddRange(transferJob.Cancel());
			}
		}
		processDownloadDataInfo.Success &= !errors.Any();
		bool success = processDownloadDataInfo.Success;
		processDownloadDataInfo.Success = success & await DeleteDownload(downloadData);
	}

	private bool GetDownloadFileBitsJobs(out List<ITransferJob> jobs)
	{
		List<Error> errors = new List<Error>();
		errors.AddRange(_transferService.GetJobs(out jobs, allUsers: false));
		if (errors.Any())
		{
			jobs = new List<ITransferJob>();
		}
		else
		{
			jobs = jobs.Where((ITransferJob j) => j.Name.StartsWith("<UCS::~") && j.Name.EndsWith("~::UCS>")).ToList();
		}
		return !errors.Any();
	}

	private bool CheckHash(string filePath, string hash, out string fileHash)
	{
		bool result = false;
		fileHash = string.Empty;
		try
		{
			using FileStream stream = File.OpenRead(filePath);
			fileHash = stream.GetSHA1Hash();
			result = fileHash == hash;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while Checking Hash for " + filePath, "CheckHash", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
		return result;
	}
}
