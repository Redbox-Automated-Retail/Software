using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "Watchdog")]
internal sealed class WatchdogPoller : ScriptPoller
{
	private int m_gcCount;

	private readonly string[] ignoreList = new string[8] { "quick-return", "qlm-unload", "thin", "unload-thin", "sync", "sync-locations", "clean-vmz", "thin-vmz" };

	protected override int PollSleep => 1800000;

	protected override string ThreadName => "Watchdog Poller";

	protected override string JobName => "job-watchdog";

	protected override string JobLabel => "Watchdog process";

	protected override ProgramPriority Priority => ProgramPriority.Normal;

	protected override bool CoreExecute()
	{
		TrimLogs();
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		if (ControllerConfiguration.Instance.ResetTimeoutCountersWeekly)
		{
			IExecutionContext executionContext = service.ScheduleJob("reset-counters-job", string.Empty, null, ProgramPriority.Low);
			executionContext.Pend();
			executionContext.WaitForCompletion();
			executionContext.Trash();
		}
		else
		{
			LogHelper.Instance.Log("Watchdog is not configured to reset timeout counters weekly.");
		}
		if (++m_gcCount > 2 && service.GetActiveContext() == null)
		{
			GC.Collect();
			m_gcCount = 0;
		}
		CleanErroredJobs();
		CleanupJobs();
		ExecutionEngine.Instance.CleanupJobs(force: false);
		return true;
	}

	private WatchdogPoller()
	{
	}

	private void ForeachJob(Action<IExecutionContext> action)
	{
		_ = DateTime.Now;
		List<IExecutionContext> jobList = ExecutionEngine.Instance.GetJobList();
		try
		{
			jobList.ForEach(delegate(IExecutionContext job)
			{
				if (!job.IsImmediate)
				{
					action(job);
				}
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Watchdog: Foreachjob caught an exception", e);
		}
	}

	private void TrimLogs()
	{
		int num = 0;
		DateTime now = DateTime.Now;
		if (now.DayOfWeek != DayOfWeek.Sunday || now.Hour < 2 || now.Hour > 4)
		{
			return;
		}
		string[] directories = Directory.GetDirectories(ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().LogsBasePath);
		foreach (string text in directories)
		{
			if (!text.Contains("Service") && !text.Contains("ErrorLogs"))
			{
				num += TrimFilesInSubdirectory(text, "*.log", ControllerConfiguration.Instance.ApplogDayAge);
			}
		}
		num += TrimFilesInSubdirectory("c:\\Program Files\\Redbox\\HALService\\Video", "*.jpg", ControllerConfiguration.Instance.WatchdogImageDayAge);
		LogHelper.Instance.Log("Watchdog removed {0} files", num);
	}

	private int TrimFilesInSubdirectory(string directory, string filePattern, int ageInDays)
	{
		int num = 0;
		string[] files = Directory.GetFiles(directory, filePattern);
		foreach (string text in files)
		{
			FileInfo fileInfo = new FileInfo(text);
			if (DateTime.Now.Subtract(fileInfo.LastWriteTime).Days < ageInDays)
			{
				continue;
			}
			try
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log($"Deleting file {text}", LogEntryType.Debug);
				}
				File.Delete(text);
				num++;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to delete file {text}", e);
			}
		}
		if (num > 0)
		{
			LogHelper.Instance.Log("AppLog Trimmer: deleted {0} files of type {1} from {2}", num, (filePattern == "*.log") ? "Application Log" : "image", directory);
		}
		return num;
	}

	private bool JobIsIgnored(IExecutionContext context)
	{
		if (ignoreList.Length == 0)
		{
			return false;
		}
		string[] array = ignoreList;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Equals(context.ProgramName, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private void CleanupJobs()
	{
		DateTime now = DateTime.Now;
		int time = 1;
		int trashed = 0;
		ForeachJob(delegate(IExecutionContext job)
		{
			ExecutionContextStatus status = job.GetStatus();
			if ((ExecutionContextStatus.Stopped == status || ExecutionContextStatus.Completed == status) && !job.IsConnected && !JobIsIgnored(job) && job.ExecutionStart.HasValue)
			{
				DateTime dateTime = job.ExecutionStart.Value;
				if (job.Result.ExecutionTime.HasValue)
				{
					dateTime = dateTime.Add(job.Result.ExecutionTime.Value);
				}
				else
				{
					int num = time + 1;
					time = num;
				}
				if (now >= dateTime.AddHours(time))
				{
					int num = trashed + 1;
					trashed = num;
					job.Trash();
				}
			}
		});
		LogHelper.Instance.Log("Watchdog trashed {0} jobs.", trashed);
	}

	private void CleanErroredJobs()
	{
		DateTime now = DateTime.Now;
		int cleaned = 0;
		ForeachJob(delegate(IExecutionContext job)
		{
			if (job.GetStatus() == ExecutionContextStatus.Errored)
			{
				DateTime createdOn = job.CreatedOn;
				if (now >= createdOn.AddDays(5.0))
				{
					int num = cleaned + 1;
					cleaned = num;
					job.Trash();
				}
			}
		});
		LogHelper.Instance.Log("Watchdog trashed {0} errored jobs.", cleaned);
	}
}
