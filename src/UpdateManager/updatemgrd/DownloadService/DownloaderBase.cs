using System;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.DownloadService;

internal class DownloaderBase : IDownloader
{
	private DownloadData _downloadData;

	public DownloadData DownloadData
	{
		get
		{
			return _downloadData;
		}
		private set
		{
			_downloadData = value;
		}
	}

	public DownloaderBase(DownloadData downloadData)
	{
		_downloadData = downloadData;
	}

	public virtual ErrorList ProcessDownload()
	{
		return new ErrorList();
	}

	public virtual ErrorList DeleteDownload()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			DownloadFactory.Instance.DeleteDownload(DownloadData);
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("DownloaderBase", "Unhandled exception occurred DeleteDownload", e));
		}
		return errorList;
	}

	public virtual ErrorList SaveDownload()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			DownloadFactory.Instance.SaveDownload(DownloadData);
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("DownloaderBase", "Unhandled exception occurred SaveDownload", e));
		}
		return errorList;
	}

	public ErrorList Complete()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			DownloadData.DownloadState = DownloadState.Complete;
			SaveDownload();
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("DownloaderBase", "An unhandled exception occurred in Cleanup.", e));
		}
		return errorList;
	}
}
