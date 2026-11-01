using System;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class Win32TaskSchedulerFunctions
{
	[KernelFunction(Name = "kernel.win32removescheduledtask")]
	internal static bool RemoveScheduledTask(string name)
	{
		return ServiceLocator.Instance.GetService<IWindowsTaskScheduler>()?.RemoveScheduledTask(name) ?? false;
	}

	[KernelFunction(Name = "kernel.win32getscheduledtasknames")]
	internal static LuaTable GetScheduledTaskNames()
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		IWindowsTaskScheduler windowsTaskScheduler = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (windowsTaskScheduler == null)
		{
			return luaTable;
		}
		int index = 1;
		foreach (string scheduledTask in windowsTaskScheduler.GetScheduledTasks())
		{
			luaTable[index++] = scheduledTask;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.win32createscheduledtask")]
	internal static void CreateScheduledTask(string name, string applicationName, string arguments, string workingDirectory, string comment, string creatorName, string accountName, string password)
	{
		ServiceLocator.Instance.GetService<IWindowsTaskScheduler>()?.CreateScheduledTask(name, applicationName, arguments, workingDirectory, comment, creatorName, accountName, password);
	}

	[KernelFunction(Name = "kernel.win32addonidletrigger")]
	internal static void AddOnIdleTrigger(string name, object beginDate, object endDate, object idleWaitMinutes)
	{
		IWindowsTaskScheduler windowsTaskScheduler = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (windowsTaskScheduler == null)
		{
			return;
		}
		DateTime? begin = null;
		if (beginDate is LuaTable beginTable)
		{
			begin = beginTable.ToDateTime();
		}
		if (begin.HasValue)
		{
			DateTime? end = null;
			if (endDate is LuaTable endTable)
			{
				end = endTable.ToDateTime();
			}
			short? idleWait = null;
			if (idleWaitMinutes != null)
			{
				idleWait = Convert.ToInt16(idleWaitMinutes);
			}
			windowsTaskScheduler.AddOnIdleTrigger(name, begin.Value, end, idleWait);
		}
	}

	[KernelFunction(Name = "kernel.win32addondailytrigger")]
	internal static void AddDailyTrigger(string name, object beginDate, object endDate, object hours, object minutes, object daysInterval, object minutesInterval)
	{
		IWindowsTaskScheduler windowsTaskScheduler = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (windowsTaskScheduler == null)
		{
			return;
		}
		DateTime? begin = null;
		if (beginDate is LuaTable beginTable)
		{
			begin = beginTable.ToDateTime();
		}
		if (begin.HasValue)
		{
			DateTime? end = null;
			if (endDate is LuaTable endTable)
			{
				end = endTable.ToDateTime();
			}
			int startHour = DateTime.Now.Hour;
			if (hours != null)
			{
				startHour = Convert.ToInt16(hours);
			}
			int startMinute = DateTime.Now.Minute;
			if (minutes != null)
			{
				startMinute = Convert.ToInt16(minutes);
			}
			int dayInterval = 1;
			if (daysInterval != null)
			{
				dayInterval = Convert.ToInt16(daysInterval);
			}
			int? repeatMinutes = null;
			if (minutesInterval != null)
			{
				repeatMinutes = Convert.ToInt32(minutesInterval);
			}
			windowsTaskScheduler.AddDailyTrigger(name, begin.Value, end, (short)startHour, (short)startMinute, (short)dayInterval, repeatMinutes);
		}
	}

	[KernelFunction(Name = "kernel.win32addonsystemstartuptrigger")]
	internal static void AddSystemStartupTrigger(string name, object beginDate, object endDate)
	{
		IWindowsTaskScheduler windowsTaskScheduler = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (windowsTaskScheduler == null)
		{
			return;
		}
		DateTime? begin = null;
		if (beginDate is LuaTable beginTable)
		{
			begin = beginTable.ToDateTime();
		}
		if (begin.HasValue)
		{
			DateTime? end = null;
			if (endDate is LuaTable endTable)
			{
				end = endTable.ToDateTime();
			}
			windowsTaskScheduler.AddSystemStartupTrigger(name, begin.Value, end);
		}
	}
}
