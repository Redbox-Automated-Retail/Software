using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.DownloadService;

internal class DownloadService : IDownloadService
{
	private Timer _timer;

	private bool _isRunning;

	private int _inDoWork;

	private string _root;

	public static DownloadService Instance => Singleton<DownloadService>.Instance;

	public ErrorList Initialize(string rootPath)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_root = rootPath;
			_timer = new Timer(delegate
			{
				DoWork();
			});
			_isRunning = false;
			ServiceLocator.Instance.AddService(typeof(IDownloadService), this);
			LogHelper.Instance.Log("Initialized the download service", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DownloadService", "Unhandled exception occurred in Initialize.", e));
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer.Change(new TimeSpan(0, 0, 35), new TimeSpan(0, 1, 0));
			_isRunning = true;
			LogHelper.Instance.Log("Starting the Download service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DownloadService", "An unhandled exception occurred.", e));
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
			LogHelper.Instance.Log("Stopping the download service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DownloadService", "An unhandled exception occurred while stopping the Download service.", e));
		}
		return errorList;
	}

	private void DoWork()
	{
		try
		{
			if (!_isRunning)
			{
				LogHelper.Instance.Log("The DownloadService service is not running.", LogEntryType.Info);
				return;
			}
			if (Interlocked.CompareExchange(ref _inDoWork, 1, 0) == 1)
			{
				LogHelper.Instance.Log("Already in DownloadService.DoWork", LogEntryType.Info);
				return;
			}
			try
			{
				LogHelper.Instance.Log("Executing DownloadService.DoWork");
				ProcessDownloads();
			}
			finally
			{
				_inDoWork = 0;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an exception in DownloadService.DoWork()", e);
		}
	}

	public List<IDownloader> GetDownloads()
	{
		return DownloadFactory.Instance.GetDownloads();
	}

	public ErrorList AddDownload(string key, string hash, string url, DownloadPriority priority, out IDownloader download)
	{
		ErrorList errorList = new ErrorList();
		download = null;
		LogHelper.Instance.Log("Adding download key: {0} hash: {1} url: {2} priority: {3}", key, hash, url, priority.ToString());
		try
		{
			IDownloader downloaderFromData = DownloadFactory.Instance.GetDownloaderFromData(DownloadFactory.Instance.GetDownloadData(key, hash, url, priority));
			errorList.AddRange(downloaderFromData.SaveDownload());
			if (errorList.ContainsError())
			{
				return errorList;
			}
			download = downloaderFromData;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DownloadService", "An unhandled exception occurred in AddDownload.", e));
		}
		return errorList;
	}

	private ErrorList ProcessDownloads()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Executing DownloadService.ProcessDownloads");
			List<IDownloader> downloads = DownloadFactory.Instance.GetDownloads();
			LogHelper.Instance.Log("DownloadService found {0} download files.", downloads.Count);
			foreach (IDownloader item in downloads)
			{
				errorList.AddRange(item.ProcessDownload());
			}
			errorList.AddRange(BitsDownloader.Cleanup());
			LogHelper.Instance.Log("Finished executing DownloadService.ProcessDownloads");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DownloadService", "An unhandled exception occurred in ProcessDownloads.", e));
		}
		return errorList;
	}

	private DownloadService()
	{
	}
}
