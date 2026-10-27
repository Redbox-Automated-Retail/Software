using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.DownloadFile;

internal class BitsDownloadFile : DownloadFileBase
{
	private const string DownloadFileTypeName = "Bits download";

	private const string BitsNameFormat = "<:~{0}-{1}~:>";

	private const string BitsNameLeft = "<:~";

	private const string BitsNameRight = "~:>";

	public BitsDownloadFile(IDownloadFileData downloadFileData)
		: base(downloadFileData)
	{
	}

	public override ErrorList ProcessDownloadFile()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.None)
		{
			errorList.AddRange(CheckNone());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PendingDownload)
		{
			errorList.AddRange(CheckPendingDownload());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Downloading)
		{
			errorList.AddRange(CheckDownloading());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostDownload)
		{
			errorList.AddRange(CheckPostDownload());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PendingInstall)
		{
			errorList.AddRange(CheckPendingInstall());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Installing)
		{
			errorList.AddRange(CheckInstalling());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostInstall)
		{
			errorList.AddRange(CheckPostInstall());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Complete)
		{
			errorList.AddRange(CheckComplete());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Error)
		{
			errorList.AddRange(CheckError());
		}
		return errorList;
	}

	private ErrorList CheckNone()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.None)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		try
		{
			if (DownloadFileDestPathExists())
			{
				errorList.AddRange(DownloadFileDestPathCheckHash(out var matched, out var _));
				if (matched)
				{
					base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PendingInstall;
					DownloadFileFactory.Instance.SaveDownloadFile(base.DownloadFileData);
					LogHelper.Instance.Log("DownloadFile Name: {0} has been put on the downloadfile queue with a pending status as file is already in it's destination.", base.DownloadFileData.Name);
					errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile already exists, skipping download.", null));
					return errorList;
				}
			}
			ITransferJob job = null;
			ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
			string text = $"<:~{base.DownloadFileData.Name}-{base.DownloadFileData.Id.ToString()}~:>";
			errorList.AddRange(service.CreateDownloadJob(text, out job));
			errorList.AddRange(job.SetMinimumRetryDelay(60u));
			errorList.AddRange(job.SetNoProgressTimeout(0u));
			errorList.AddRange(job.SetPriority(TransferJobPriority.Low));
			errorList.AddRange(job.AddItem(base.DownloadFileData.Url, Path.GetTempFileName()));
			if (errorList.ContainsError())
			{
				errorList.AddRange(job.Cancel());
				errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Error, "DownloadFile bits error, leaving status at None for repeat. RetryCount: " + base.DownloadFileData.RetryCount, errorList[0].Description));
				if (base.DownloadFileData.RetryCount == 10)
				{
					base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				}
				else
				{
					base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.None;
				}
				base.DownloadFileData.Message = errorList[0].Description;
				base.DownloadFileData.RetryCount++;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			errorList.AddRange(job.Resume());
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			base.DownloadFileData.BitsGuid = job.ID;
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Downloading;
			errorList.AddRange(SaveDownloadFile());
			LogHelper.Instance.Log("DownloadFile Name: {0} has been put on the downloadfile queue.", base.DownloadFileData.Name);
			LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", text, job.ID);
			errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile bits download started", null));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.None", e));
		}
		return errorList;
	}

	private ErrorList CheckPendingDownload()
	{
		ErrorList result = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PendingDownload)
		{
			return result;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		return result;
	}

	private ErrorList CheckDownloading()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Downloading)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		try
		{
			ServiceLocator.Instance.GetService<ITransferService>();
			List<ITransferJob> jobs = null;
			errorList.AddRange(GetDownloadFileBitsJobs(out jobs));
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == base.DownloadFileData.BitsGuid).FirstOrDefault();
			if (transferJob == null)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = "Bits job missing";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (transferJob.Status == TransferStatus.Error)
			{
				errorList.AddRange(SendTransferStatus(transferJob, success: false));
				errorList.AddRange(transferJob.Cancel());
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = "Bits job error";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (transferJob.Status == TransferStatus.Suspended)
			{
				errorList.AddRange(SendTransferStatus(transferJob, success: false));
				errorList.AddRange(transferJob.Cancel());
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = "Bits job error";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (transferJob.Status == TransferStatus.Transferred)
			{
				errorList.AddRange(FinishBitsDownload(transferJob));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.CheckDownloading.", e));
		}
		return errorList;
	}

	private ErrorList FinishBitsDownload(ITransferJob job)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log(string.Format("{0} FinishBitsDownload for {1}", "Bits download", base.DownloadFileData.Name));
			errorList.AddRange(job.GetItems(out var items));
			errorList.AddRange(job.Complete());
			if (errorList.ContainsError())
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF999", $"Job {job.ID} get items or completion failed.", "This download will be canceled."));
				errorList.AddRange(job.Cancel());
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			ITransferItem transferItem = items[0];
			string text = Path.Combine(transferItem.Path, transferItem.Name);
			errorList.AddRange(CheckHash(text, base.DownloadFileData.FileKey, out var matched, out var fileHash));
			if (!matched)
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("D996", $"Download file: {base.DownloadFileData.Name} path: {text} should have hash {base.DownloadFileData.FileKey} but it hash {fileHash}", "This download failed and will need to be restarted."));
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				File.Delete(text);
				return errorList;
			}
			LogHelper.Instance.Log("Download file: {0} matched the server hash of {1}.", base.DownloadFileData.Name, base.DownloadFileData.FileKey);
			string downloadFileDestPath = GetDownloadFileDestPath();
			try
			{
				if (File.Exists(downloadFileDestPath))
				{
					File.Delete(downloadFileDestPath);
				}
				File.Move(text, downloadFileDestPath);
				LogHelper.Instance.Log("Download File {0} succeeded and copied to {1}.", base.DownloadFileData.Name, downloadFileDestPath);
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PendingInstall;
				errorList.AddRange(SaveDownloadFile());
			}
			catch (Exception ex)
			{
				try
				{
					base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
					base.DownloadFileData.Message = ex.Message;
					errorList.AddRange(SaveDownloadFile());
					File.Delete(text);
				}
				catch
				{
				}
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS995", "An unhandled exception occurred in DownloadFileService.FinishDownload.", ex));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS990", "An unhandled exception occurred in DownloadFileService.FinishDownload.", e));
		}
		finally
		{
			errorList.AddRange(SendTransferStatus(job, errorList.Count == 0));
		}
		return errorList;
	}

	private ErrorList CheckPostDownload()
	{
		ErrorList result = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostDownload)
		{
			return result;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		return result;
	}

	private ErrorList CheckPendingInstall()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PendingInstall)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		try
		{
			LogHelper.Instance.Log("Executing DownloadFileService.CheckPending");
			ServiceLocator.Instance.GetService<IUpdateService>();
			if (!IsValidTime())
			{
				LogHelper.Instance.Log("BitsDownloadFile: CheckPendingInstall is skipping download file {0} until start: {1} and end time: {2} are valid.", base.DownloadFileData.Name, base.DownloadFileData.StartTime, base.DownloadFileData.EndTime);
				return errorList;
			}
			if (!string.IsNullOrEmpty(base.DownloadFileData.ActivateScript))
			{
				errorList.AddRange(RunScript(out var scriptCompleted));
				if (errorList.ContainsError())
				{
					base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
					base.DownloadFileData.Message = errorList[0].Description;
					errorList.AddRange(SaveDownloadFile());
					return errorList;
				}
				if (!scriptCompleted)
				{
					return errorList;
				}
			}
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PostInstall;
			errorList.AddRange(SaveDownloadFile());
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF999", "An unhandled exception occurred in BitsDownloadFile.CheckPendingInstall.", e));
		}
		return errorList;
	}

	private ErrorList CheckInstalling()
	{
		ErrorList result = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Installing)
		{
			return result;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		return result;
	}

	private ErrorList CheckPostInstall()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostInstall)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
		errorList.AddRange(SaveDownloadFile());
		return errorList;
	}

	private ErrorList CheckComplete()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Complete)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile success", null));
		errorList.AddRange(DeleteDownloadFile());
		return errorList;
	}

	private ErrorList CheckError()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Error)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Error, "DownloadFile error", base.DownloadFileData.Message));
		_ = base.DownloadFileData.BitsGuid;
		List<ITransferJob> jobs = null;
		errorList.AddRange(GetDownloadFileBitsJobs(out jobs));
		if (!errorList.ContainsError())
		{
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == base.DownloadFileData.BitsGuid).FirstOrDefault();
			if (transferJob != null)
			{
				errorList.AddRange(transferJob.Cancel());
			}
		}
		errorList.AddRange(DeleteDownloadFile());
		return errorList;
	}

	internal static ErrorList Cleanup()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Executing BitsDownloadFile.Cleanup");
			List<IDownloadFile> downloadFiles = DownloadFileFactory.Instance.GetDownloadFiles();
			List<ITransferJob> jobs = null;
			errorList.AddRange(GetDownloadFileBitsJobs(out jobs));
			if (errorList.ContainsError())
			{
				return errorList;
			}
			foreach (ITransferJob job in jobs)
			{
				if (downloadFiles.Where((IDownloadFile d) => d.DownloadFileData.BitsGuid == job.ID).FirstOrDefault() == null)
				{
					LogHelper.Instance.Log("DownloadFileService: Clearing bits job, guid {0} that has not download file changeset.", job.ID);
					errorList.AddRange(job.Cancel());
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred in BitsDownloadFile.Cleanup.", e));
		}
		return errorList;
	}

	private static ErrorList GetDownloadFileBitsJobs(out List<ITransferJob> jobs)
	{
		ErrorList errorList = new ErrorList();
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		errorList.AddRange(service.GetJobs(out jobs, allUsers: false));
		if (errorList.ContainsError())
		{
			jobs = new List<ITransferJob>();
			return errorList;
		}
		jobs = jobs.Where((ITransferJob j) => j.Name.StartsWith("<:~") && j.Name.EndsWith("~:>")).ToList();
		return errorList;
	}

	private string GetDownloadFileDestPath()
	{
		return Path.Combine(base.DownloadFileData.DestinationPath, base.DownloadFileData.FileName);
	}

	private bool DownloadFileDestPathExists()
	{
		return File.Exists(GetDownloadFileDestPath());
	}

	private ErrorList DownloadFileDestPathCheckHash(out bool matched, out string fileHash)
	{
		ErrorList errorList = new ErrorList();
		matched = false;
		fileHash = string.Empty;
		try
		{
			string downloadFileDestPath = GetDownloadFileDestPath();
			if (!string.IsNullOrEmpty(base.DownloadFileData.FileKey))
			{
				errorList.AddRange(CheckHash(downloadFileDestPath, base.DownloadFileData.FileKey, out matched, out fileHash));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.ChangeSetDestPathCheckHash.", e));
		}
		return errorList;
	}

	private ErrorList CheckHash(string filePath, string hash, out bool matched, out string fileHash)
	{
		ErrorList errorList = new ErrorList();
		matched = false;
		fileHash = string.Empty;
		try
		{
			using FileStream inputStream = File.OpenRead(filePath);
			fileHash = inputStream.ToASCIISHA1Hash();
			matched = fileHash == hash;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.CheckHash.", e));
		}
		return errorList;
	}

	private ErrorList SendTransferStatus(ITransferJob job, bool success)
	{
		double averageSpeedInKPS = 0.0;
		if (job.Status == TransferStatus.Transferred)
		{
			DateTime dateTime = (job.FinishTime.HasValue ? job.FinishTime.Value : DateTime.UtcNow);
			if ((dateTime - job.StartTime).TotalSeconds > 0.0)
			{
				averageSpeedInKPS = (double)job.TotalBytesTransfered / (dateTime - job.StartTime).TotalSeconds;
			}
		}
		var anon = new
		{
			DownloadFileId = base.DownloadFileData.Id,
			Name = base.DownloadFileData.Name,
			DownloadFileType = base.DownloadFileData.DownloadFileDataType.ToString(),
			Success = success,
			TotalBytesTransfered = job.TotalBytesTransfered,
			AverageSpeedInKPS = averageSpeedInKPS,
			DownloadFileState = base.DownloadFileData.DownloadFileDataState.ToString()
		};
		return SendStatus(anon.Success ? StatusMessage.StatusMessageType.Info : StatusMessage.StatusMessageType.Error, "DownloadFile transfer status", anon);
	}
}
