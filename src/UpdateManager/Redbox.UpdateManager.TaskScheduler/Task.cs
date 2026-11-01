using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Quartz;
using Quartz.Util;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.TaskScheduler;

public class Task : ITask, IJob
{
	internal int InExecution;

	private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private static int _lockTimeout = 3000;

	public ScheduleType ScheduleType { get; set; }

	public string CronExpression { get; set; }

	public string Name { get; set; }

	public TimeSpan? StartOffset { get; set; }

	public DateTime? StartTime { get; set; }

	public DateTime? EndTime { get; set; }

	public TimeSpan? RepeatInterval { get; set; }

	public string Payload { get; set; }

	public PayloadType PayloadType { get; set; }

	public Task()
	{
	}

	public Task(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
	{
		Name = name;
		Payload = payload;
		PayloadType = payloadType;
		StartOffset = startOffset;
		StartTime = startTime;
		EndTime = endTime;
		ScheduleType = ScheduleType.Cron;
		CronExpression = cronExpression;
	}

	public Task(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
	{
		Name = name;
		Payload = payload;
		PayloadType = payloadType;
		StartOffset = startOffset;
		StartTime = startTime;
		EndTime = endTime;
		ScheduleType = ScheduleType.Simple;
		RepeatInterval = repeatInterval;
	}

	public void Execute(IJobExecutionContext context)
	{
		try
		{
			Task task;
			if (context == null)
			{
				task = this;
			}
			else
			{
				task = ((DirtyFlagMap<string, object>)(object)context.JobDetail.JobDataMap)["Task"] as Task;
				if (task == null)
				{
					LogHelper.Instance.Log("WARNING: JobExecutionContext did not have a valid Task in the JobDataMap.");
					return;
				}
			}
			if (Interlocked.CompareExchange(ref task.InExecution, 1, 0) == 1)
			{
				LogHelper.Instance.Log($"Task: {task.Payload} already executing.");
				return;
			}
			try
			{
				ExecuteEvent(task);
			}
			finally
			{
				task.InExecution = 0;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was thrown from Task.Execute.", e);
		}
	}

	public new string ToString()
	{
		return $"Name:{Name} Type:{ScheduleType} PayloadType:{PayloadType} @ {((ScheduleType == ScheduleType.Cron) ? CronExpression : RepeatInterval.ToString())}";
	}

	private static void ExecuteEvent(ITask task)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("Task.ExecuteEvent", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
			return;
		}
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			LogHelper.Instance.Log($"SCHEDULER: *** Begin Schedule Event: {task.Payload}", LogEntryType.Info);
			try
			{
				switch (task.PayloadType)
				{
				case PayloadType.Script:
				{
					string text3 = task.Payload;
					if (!Path.IsPathRooted(text3))
					{
						text3 = Path.Combine(service.ExpandProperties("${Scripts}"), task.Payload);
					}
					if (!File.Exists(text3))
					{
						LogHelper.Instance.Log("{0} not found.", text3);
						return;
					}
					try
					{
						if (string.Compare(task.Payload, "watchdog.lua", StringComparison.CurrentCultureIgnoreCase) == 0)
						{
							ServiceLocator.Instance.GetService<IHealthService>().Update("UPDATE_SERVICE_WATCHDOG");
						}
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log("(TASK0099) - An unhandled exception occurred updating health for watchdog.", e);
					}
					IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
					string chunk = File.ReadAllText(text3);
					LogHelper.Instance.Log("Executing lua script at {0}", text3);
					kernelService.ExecuteChunk(chunk);
					break;
				}
				case PayloadType.Shell:
				{
					string text;
					string text2;
					if (task.Payload.Contains(" "))
					{
						text = task.Payload.Substring(0, task.Payload.IndexOf(" "));
						text2 = task.Payload.Substring(task.Payload.IndexOf(" ") + 1);
					}
					else
					{
						text = task.Payload;
						text2 = string.Empty;
					}
					LogHelper.Instance.Log("Executing shell command process: {0} arguments: {1}", text, text2);
					using (Process process = Process.Start(new ProcessStartInfo
					{
						WorkingDirectory = service.ExpandProperties("${RunningPath}"),
						ErrorDialog = false,
						UseShellExecute = false,
						Arguments = text2,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						FileName = text
					}))
					{
						LogHelper.Instance.Log("Process '{0}' started with PID '{1}'.", process.ProcessName, process.Id);
					}
					break;
				}
				case PayloadType.ServerPoll:
					LogHelper.Instance.Log("Running scheduled ServerPoll");
					foreach (Redbox.UpdateManager.ComponentModel.Error item in ServiceLocator.Instance.GetService<IUpdateService>().ServerPoll())
					{
						LogHelper.Instance.Log(item.ToString(), LogEntryType.Error);
					}
					break;
				}
				ServiceLocator.Instance.GetService<IKernelService>().PerformShutdown();
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in Task ExecuteEvent.", e2);
			}
			finally
			{
				LogHelper.Instance.Log($"SCHEDULER: *** End Schedule Event: {task.Payload}, Execution Time = {executionTimer.Elapsed}", LogEntryType.Info);
			}
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}
}
