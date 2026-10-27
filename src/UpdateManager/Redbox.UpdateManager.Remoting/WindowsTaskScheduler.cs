using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using TaskScheduler;

namespace Redbox.UpdateManager.Remoting;

public class WindowsTaskScheduler : IWindowsTaskScheduler
{
	public static WindowsTaskScheduler Instance => Singleton<WindowsTaskScheduler>.Instance;

	public void Initialize()
	{
		ServiceLocator.Instance.AddService(typeof(IWindowsTaskScheduler), this);
	}

	public ReadOnlyCollection<string> GetScheduledTasks()
	{
		using ScheduledTasks scheduledTasks = new ScheduledTasks();
		return new List<string>(scheduledTasks.GetTaskNames()).AsReadOnly();
	}

	public bool RemoveScheduledTask(string name)
	{
		try
		{
			using ScheduledTasks scheduledTasks = new ScheduledTasks();
			return scheduledTasks.DeleteTask(name);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.RemoveScheduledTask.", e);
			return false;
		}
	}

	public void CreateScheduledTask(string name, string applicationName, string arguments, string workingDirectory, string comment, string creatorName, string accountName, string password)
	{
		try
		{
			using ScheduledTasks scheduledTasks = new ScheduledTasks();
			Task task = null;
			try
			{
				task = scheduledTasks.OpenTask(name) ?? scheduledTasks.CreateTask(name);
				task.Flags = TaskFlags.Hidden;
				if (string.IsNullOrEmpty(password))
				{
					task.Flags |= TaskFlags.RunOnlyIfLoggedOn;
				}
				task.Priority = ProcessPriorityClass.Normal;
				task.ApplicationName = applicationName;
				if (!string.IsNullOrEmpty(arguments))
				{
					task.Parameters = arguments;
				}
				if (!string.IsNullOrEmpty(workingDirectory))
				{
					task.WorkingDirectory = workingDirectory;
				}
				if (!string.IsNullOrEmpty(comment))
				{
					task.Comment = comment;
				}
				if (!string.IsNullOrEmpty(creatorName))
				{
					task.Creator = creatorName;
				}
				if (!string.IsNullOrEmpty(accountName))
				{
					if (string.Compare(accountName, "system", ignoreCase: true) == 0)
					{
						task.SetAccountInformation("", (string)null);
					}
					else
					{
						task.SetAccountInformation(accountName, password);
					}
				}
				task.Save();
			}
			finally
			{
				task?.Dispose();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.CreateScheduledTask.", e);
		}
	}

	public void AddOnIdleTrigger(string name, DateTime beginDate, DateTime? endDate, short? idleWaitMinutes)
	{
		try
		{
			using ScheduledTasks scheduledTasks = new ScheduledTasks();
			Task task = null;
			try
			{
				task = scheduledTasks.OpenTask(name);
				if (task != null)
				{
					task.IdleWaitMinutes = idleWaitMinutes ?? 1;
					OnIdleTrigger onIdleTrigger = new OnIdleTrigger
					{
						BeginDate = beginDate
					};
					if (endDate.HasValue)
					{
						onIdleTrigger.EndDate = endDate.Value;
					}
					task.Triggers.Add(onIdleTrigger);
					task.Save();
				}
			}
			finally
			{
				task?.Dispose();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.AddOnIdleTrigger.", e);
		}
	}

	public void AddDailyTrigger(string name, DateTime beginDate, DateTime? endDate, short hours, short minutes, short? daysInterval, int? minutesInterval)
	{
		try
		{
			using ScheduledTasks scheduledTasks = new ScheduledTasks();
			Task task = null;
			try
			{
				task = scheduledTasks.OpenTask(name);
				if (task != null)
				{
					DailyTrigger dailyTrigger = new DailyTrigger(hours, minutes)
					{
						BeginDate = beginDate
					};
					if (daysInterval.HasValue)
					{
						dailyTrigger.DaysInterval = daysInterval.Value;
					}
					if (endDate.HasValue)
					{
						dailyTrigger.EndDate = endDate.Value;
					}
					if (minutesInterval.HasValue)
					{
						dailyTrigger.DurationMinutes = 1440;
						dailyTrigger.IntervalMinutes = minutesInterval.Value;
					}
					task.Triggers.Add(dailyTrigger);
					task.Save();
				}
			}
			finally
			{
				task?.Dispose();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.AddDailyTrigger.", e);
		}
	}

	public void AddSystemStartupTrigger(string name, DateTime beginDate, DateTime? endDate)
	{
		try
		{
			using ScheduledTasks scheduledTasks = new ScheduledTasks();
			Task task = null;
			try
			{
				task = scheduledTasks.OpenTask(name);
				if (task != null)
				{
					OnSystemStartTrigger onSystemStartTrigger = new OnSystemStartTrigger
					{
						BeginDate = beginDate
					};
					if (endDate.HasValue)
					{
						onSystemStartTrigger.EndDate = endDate.Value;
					}
					task.Triggers.Add(onSystemStartTrigger);
					task.Save();
				}
			}
			finally
			{
				task?.Dispose();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.AddSystemStartupTrigger.", e);
		}
	}

	private WindowsTaskScheduler()
	{
	}
}
