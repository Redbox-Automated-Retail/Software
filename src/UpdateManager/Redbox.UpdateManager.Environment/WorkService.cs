using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class WorkService : IPollRequestReply
{
	private class ResultEntry
	{
		public string Entry { get; set; }

		public long ID { get; set; }
	}

	private Timer m_workTimer;

	private bool m_isRunning;

	private int m_inDoWork;

	private const string WorkExtension = ".workdat";

	private const string ScriptLabel = "script";

	private const string InCompleteLabel = ".incomplete";

	public static WorkService Instance => Singleton<WorkService>.Instance;

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			m_workTimer = new Timer(delegate
			{
				DoWork();
			});
			m_isRunning = false;
			LogHelper.Instance.Log("Initialized the work service", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS999", "An unhandled exception occurred while initializing the work service.", e));
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		if (m_isRunning)
		{
			LogHelper.Instance.Log("The work service is already running.", LogEntryType.Info);
			return errorList;
		}
		try
		{
			m_workTimer.Change(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0));
			m_isRunning = true;
			LogHelper.Instance.Log("Starting the work service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS998", "An unhandled exception occurred while starting the work service.", e));
		}
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			m_workTimer.Change(-1, -1);
			m_isRunning = false;
			LogHelper.Instance.Log("Stopping the work service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS998", "An unhandled exception occurred while stopping the work service.", e));
		}
		return errorList;
	}

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList result = new ErrorList();
		pollRequestList = new List<PollRequest>();
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		string storeNumber = ServiceLocator.Instance.GetService<IUpdateService>().StoreNumber();
		for (ResultEntry resultEntry = service.Peek<ResultEntry>("script"); resultEntry != null; resultEntry = service.Peek<ResultEntry>("script"))
		{
			Dictionary<object, object> results = resultEntry.Entry.ToObject<Dictionary<object, object>>();
			WorkResultPollRequestDTO instance = new WorkResultPollRequestDTO
			{
				StoreNumber = storeNumber,
				ScriptID = resultEntry.ID,
				Results = results
			};
			PollRequest item = new PollRequest
			{
				PollRequestType = PollRequestType.WorkResult,
				Data = instance.ToJson()
			};
			pollRequestList.Add(item);
			service.Dequeue("script");
			LogHelper.Instance.Log("Sent result for script {0} to server.", resultEntry.ID);
		}
		return result;
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
			IEnumerable<ClientWorkScript> enumerable = pollReplyList.Select((PollReply pollReply) => pollReply.Data.ToObject<ClientWorkScript>());
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			List<Guid> list = new List<Guid>();
			foreach (ClientWorkScript item in enumerable)
			{
				Guid guid = Guid.NewGuid();
				list.Add(guid);
				service.Set(string.Concat(guid, ".workdat"), item);
				LogHelper.Instance.Log("Script Id: {0} has been put on the work queue.", item.ID);
			}
			service.Set(".incomplete", list);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.", e));
		}
		return errorList;
	}

	private WorkService()
	{
	}

	private void DoWork()
	{
		try
		{
			if (!m_isRunning)
			{
				LogHelper.Instance.Log("The work service is not running.", LogEntryType.Info);
				return;
			}
			if (Interlocked.CompareExchange(ref m_inDoWork, 1, 0) == 1)
			{
				LogHelper.Instance.Log("Already in DoWork", LogEntryType.Info);
				return;
			}
			try
			{
				DoIncompleteAndQueuedWork().ToLogHelper();
			}
			finally
			{
				m_inDoWork = 0;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an exception in DoWork()", e);
		}
	}

	private static ErrorList DoIncompleteAndQueuedWork()
	{
		ErrorList errorList = new ErrorList();
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		List<Guid> list = ServiceLocator.Instance.GetService<IDataStoreService>().Get<List<Guid>>(".incomplete");
		if (list != null && list.Count > 0)
		{
			LogHelper.Instance.Log("Doing {0} incomplete and queued work items.", list.Count);
			errorList.AddRange(service.DoInCompleteWork(list));
		}
		else
		{
			LogHelper.Instance.Log("No incomplete or queued work items found.");
		}
		return errorList;
	}
}
