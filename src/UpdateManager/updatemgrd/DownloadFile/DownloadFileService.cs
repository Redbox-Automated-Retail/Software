using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.DownloadFile;

internal class DownloadFileService : IPollRequestReply, IDownloadFileService
{
	private bool _rebootRequired;

	private Timer _timer;

	private bool _isRunning;

	private int _inDoWork;

	public static DownloadFileService Instance => Singleton<DownloadFileService>.Instance;

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

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer = new Timer(delegate
			{
				DoWork();
			});
			_isRunning = false;
			ServiceLocator.Instance.AddService(typeof(IDownloadFileService), this);
			LogHelper.Instance.Log("Initialized the download file service", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred while initializing the download file service.", e));
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer.Change(new TimeSpan(0, 0, 55), new TimeSpan(0, 5, 0));
			_isRunning = true;
			LogHelper.Instance.Log("Starting the download file service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS998", "An unhandled exception occurred while starting the download file service.", e));
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
			LogHelper.Instance.Log("Stopping the download file service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS997", "An unhandled exception occurred while stopping the download file service.", e));
		}
		return errorList;
	}

	public ErrorList GetPollRequest(out List<PollRequest> pollRequests)
	{
		pollRequests = new List<PollRequest>();
		return new ErrorList();
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
			IEnumerable<DownloadFileChangeSetData> changeSets = pollReplyList.Select((PollReply pollReply) => pollReply.Data.ToObject<DownloadFileChangeSetData>());
			errorList.AddRange(CreateDownloads(changeSets));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS0010", "An unhandled exception occurred in DownloadFileService.ProcessPollReply.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private DownloadFileService()
	{
	}

	private void DoWork()
	{
		try
		{
			if (!_isRunning)
			{
				LogHelper.Instance.Log("The downloadfile service is not running.", LogEntryType.Info);
				return;
			}
			if (Interlocked.CompareExchange(ref _inDoWork, 1, 0) == 1)
			{
				LogHelper.Instance.Log("Already in DownloadFileService.DoWork", LogEntryType.Info);
				return;
			}
			try
			{
				LogHelper.Instance.Log("Executing DownloadFileService.DoWork");
				ProcessDownloads().ToLogHelper();
			}
			finally
			{
				_inDoWork = 0;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an exception in DownloadFileService.DoWork()", e);
		}
	}

	private ErrorList CreateDownloads(IEnumerable<DownloadFileChangeSetData> changeSets)
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Executing DownloadFileService.CreateDownloads");
		try
		{
			foreach (DownloadFileChangeSetData changeSet in changeSets)
			{
				DownloadFileFactory.Instance.GetDownloadFileFromChangeSet(changeSet).SaveDownloadFile();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in DownloadFileService.CreateDownloads.", e));
		}
		return errorList;
	}

	private ErrorList ProcessDownloads()
	{
		ErrorList errors = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Executing DownloadFileService.ProcessDownloads");
			ServiceLocator.Instance.GetService<IKernelService>().Execute(delegate
			{
				List<IDownloadFile> downloadFiles = DownloadFileFactory.Instance.GetDownloadFiles();
				LogHelper.Instance.Log("Found {0} download files.", downloadFiles.Count);
				foreach (IDownloadFile item in downloadFiles)
				{
					errors.AddRange(item.ProcessDownloadFile());
				}
				errors.AddRange(BitsDownloadFile.Cleanup());
				if (RebootRequired)
				{
					errors.AddRange(Reboot());
				}
			});
			LogHelper.Instance.Log("Finished executing DownloadFileService.ProcessDownloads");
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred in DownloadFileService.ProcessDownloads.", e));
		}
		return errors;
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
