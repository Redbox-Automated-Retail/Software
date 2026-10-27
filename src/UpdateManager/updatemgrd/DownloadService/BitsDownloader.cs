using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.DownloadService;

internal class BitsDownloader : DownloaderBase
{
	private const string DownloadTypeName = "Bits downloader";

	private const string BitsNameFormat = "<::~{0}~::>";

	private const string BitsNameLeft = "<::~";

	private const string BitsNameRight = "~::>";

	public BitsDownloader(DownloadData downloadData)
		: base(downloadData)
	{
	}

	public override ErrorList ProcessDownload()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadData.DownloadState == DownloadState.None)
		{
			errorList.AddRange(CheckNone());
		}
		if (base.DownloadData.DownloadState == DownloadState.Downloading)
		{
			errorList.AddRange(CheckDownloading());
		}
		if (base.DownloadData.DownloadState == DownloadState.PostDownload)
		{
			errorList.AddRange(CheckPostDownload());
		}
		if (base.DownloadData.DownloadState == DownloadState.Complete)
		{
			errorList.AddRange(CheckComplete());
		}
		if (base.DownloadData.DownloadState == DownloadState.Error)
		{
			errorList.AddRange(CheckError());
		}
		return errorList;
	}

	private TransferJobPriority GetPriority()
	{
		if (base.DownloadData.DownloadPriority == DownloadPriority.Low)
		{
			return TransferJobPriority.Low;
		}
		if (base.DownloadData.DownloadPriority == DownloadPriority.Normal)
		{
			return TransferJobPriority.Normal;
		}
		if (base.DownloadData.DownloadPriority == DownloadPriority.High)
		{
			return TransferJobPriority.High;
		}
		if (base.DownloadData.DownloadPriority == DownloadPriority.Foreground)
		{
			return TransferJobPriority.Foreground;
		}
		return TransferJobPriority.Low;
	}

	private ErrorList CheckNone()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadData.DownloadState != DownloadState.None)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits downloader", base.DownloadData.DownloadState.ToString(), base.DownloadData.Key));
		try
		{
			ITransferJob job = null;
			ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
			string text = $"<::~{base.DownloadData.Key}~::>";
			errorList.AddRange(service.CreateDownloadJob(text, out job));
			errorList.AddRange(job.SetMinimumRetryDelay(60u));
			errorList.AddRange(job.SetNoProgressTimeout(0u));
			errorList.AddRange(job.SetPriority(GetPriority()));
			errorList.AddRange(job.AddItem(base.DownloadData.Url, Path.GetTempFileName()));
			if (errorList.ContainsError())
			{
				errorList.AddRange(job.Cancel());
				if (base.DownloadData.RetryCount == 10)
				{
					base.DownloadData.DownloadState = DownloadState.Error;
				}
				else
				{
					base.DownloadData.DownloadState = DownloadState.None;
				}
				base.DownloadData.Message = errorList[0].Description;
				base.DownloadData.RetryCount++;
				errorList.AddRange(SaveDownload());
				return errorList;
			}
			errorList.AddRange(job.Resume());
			if (errorList.ContainsError())
			{
				base.DownloadData.DownloadState = DownloadState.Error;
				base.DownloadData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownload());
				return errorList;
			}
			base.DownloadData.BitsGuid = job.ID;
			base.DownloadData.DownloadState = DownloadState.Downloading;
			errorList.AddRange(SaveDownload());
			LogHelper.Instance.Log("Download Name: {0} has been put on the download queue.", base.DownloadData.Key);
			LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", text, job.ID);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BitsDownloader", "An unhandled exception occurred in CheckNone", e));
		}
		return errorList;
	}

	private ErrorList CheckDownloading()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadData.DownloadState != DownloadState.Downloading)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits downloader", base.DownloadData.DownloadState.ToString(), base.DownloadData.Key));
		try
		{
			ServiceLocator.Instance.GetService<ITransferService>();
			List<ITransferJob> jobs = null;
			errorList.AddRange(GetDownloadFileBitsJobs(out jobs));
			if (errorList.ContainsError())
			{
				base.DownloadData.DownloadState = DownloadState.Error;
				base.DownloadData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownload());
				return errorList;
			}
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == base.DownloadData.BitsGuid).FirstOrDefault();
			if (transferJob == null)
			{
				base.DownloadData.DownloadState = DownloadState.Error;
				base.DownloadData.Message = "Bits job missing";
				errorList.AddRange(SaveDownload());
				return errorList;
			}
			if (transferJob.Status == TransferStatus.Error)
			{
				errorList.AddRange(transferJob.Cancel());
				base.DownloadData.DownloadState = DownloadState.Error;
				base.DownloadData.Message = "Bits job error";
				errorList.AddRange(SaveDownload());
				return errorList;
			}
			if (transferJob.Status == TransferStatus.Suspended)
			{
				errorList.AddRange(transferJob.Cancel());
				base.DownloadData.DownloadState = DownloadState.Error;
				base.DownloadData.Message = "Bits job error";
				errorList.AddRange(SaveDownload());
				return errorList;
			}
			if (transferJob.Status == TransferStatus.Transferred)
			{
				errorList.AddRange(FinishBitsDownload(transferJob));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BitsDownloader", "An unhandled exception occurred in CheckDownloading.", e));
		}
		return errorList;
	}

	private ErrorList FinishBitsDownload(ITransferJob job)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log(string.Format("{0} FinishBitsDownload for {1}", "Bits downloader", base.DownloadData.Key));
			errorList.AddRange(job.GetItems(out var items));
			errorList.AddRange(job.Complete());
			if (errorList.ContainsError())
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BitsDownloader", $"Job {job.ID} get items or completion failed.", "This download will be canceled."));
				errorList.AddRange(job.Cancel());
				base.DownloadData.DownloadState = DownloadState.Error;
				base.DownloadData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownload());
				return errorList;
			}
			ITransferItem transferItem = items[0];
			string text = Path.Combine(transferItem.Path, transferItem.Name);
			errorList.AddRange(CheckHash(text, base.DownloadData.Hash, out var matched, out var fileHash));
			if (!matched)
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BitsDownloader", $"Download file: {base.DownloadData.Key} path: {text} should have hash {base.DownloadData.Hash} but it hash {fileHash}", "This download failed and will need to be restarted."));
				base.DownloadData.DownloadState = DownloadState.Error;
				base.DownloadData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownload());
				File.Delete(text);
				return errorList;
			}
			base.DownloadData.Path = text;
			base.DownloadData.DownloadState = DownloadState.PostDownload;
			errorList.AddRange(SaveDownload());
			LogHelper.Instance.Log("Download file: {0} matched the server hash of {1}.", base.DownloadData.Key, base.DownloadData.Hash);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BitsDownloader", "An unhandled exception occurred in FinishBitsDownload.", e));
		}
		return errorList;
	}

	private ErrorList CheckPostDownload()
	{
		ErrorList result = new ErrorList();
		if (base.DownloadData.DownloadState != DownloadState.PostDownload)
		{
			return result;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits downloader", base.DownloadData.DownloadState.ToString(), base.DownloadData.Key));
		return result;
	}

	private ErrorList CheckComplete()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadData.DownloadState != DownloadState.Complete)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits downloader", base.DownloadData.DownloadState.ToString(), base.DownloadData.Key));
		errorList.AddRange(DeleteDownload());
		return errorList;
	}

	private ErrorList CheckError()
	{
		ErrorList errorList = new ErrorList();
		if (base.DownloadData.DownloadState != DownloadState.Error)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "Bits downloader", base.DownloadData.DownloadState.ToString(), base.DownloadData.Key));
		_ = base.DownloadData.BitsGuid;
		List<ITransferJob> jobs = null;
		errorList.AddRange(GetDownloadFileBitsJobs(out jobs));
		if (!errorList.ContainsError())
		{
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == base.DownloadData.BitsGuid).FirstOrDefault();
			if (transferJob != null)
			{
				errorList.AddRange(transferJob.Cancel());
			}
		}
		errorList.AddRange(DeleteDownload());
		return errorList;
	}

	internal static ErrorList Cleanup()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Executing BitsDownloader.Cleanup");
			List<IDownloader> downloads = DownloadFactory.Instance.GetDownloads();
			List<ITransferJob> jobs = null;
			errorList.AddRange(GetDownloadFileBitsJobs(out jobs));
			if (errorList.ContainsError())
			{
				return errorList;
			}
			foreach (ITransferJob job in jobs)
			{
				if (downloads.Where((IDownloader d) => d.DownloadData.BitsGuid == job.ID).FirstOrDefault() == null)
				{
					LogHelper.Instance.Log("BitsDownloader: Clearing bits job, guid {0} that has no download file changeset.", job.ID);
					errorList.AddRange(job.Cancel());
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BitsDownloader", "An unhandled exception occurred in BitsDownloadFile.Cleanup.", e));
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
		jobs = jobs.Where((ITransferJob j) => j.Name.StartsWith("<::~") && j.Name.EndsWith("~::>")).ToList();
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
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BitsDownloader", "An unhandled exception occurred in CheckHash.", e));
		}
		return errorList;
	}
}
