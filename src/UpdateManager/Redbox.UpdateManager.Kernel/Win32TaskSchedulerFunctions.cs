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
		IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (service == null)
		{
			return luaTable;
		}
		int num = 1;
		foreach (string scheduledTask in service.GetScheduledTasks())
		{
			luaTable[num++] = scheduledTask;
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
		IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (service == null)
		{
			return;
		}
		DateTime? dateTime = null;
		if (beginDate is LuaTable luaTable)
		{
			dateTime = luaTable.ToDateTime();
		}
		if (dateTime.HasValue)
		{
			DateTime? endDate2 = null;
			if (endDate is LuaTable luaTable2)
			{
				endDate2 = luaTable2.ToDateTime();
			}
			short? idleWaitMinutes2 = null;
			if (idleWaitMinutes != null)
			{
				idleWaitMinutes2 = Convert.ToInt16(idleWaitMinutes);
			}
			service.AddOnIdleTrigger(name, dateTime.Value, endDate2, idleWaitMinutes2);
		}
	}

	[KernelFunction(Name = "kernel.win32addondailytrigger")]
	internal static void AddDailyTrigger(string name, object beginDate, object endDate, object hours, object minutes, object daysInterval, object minutesInterval)
	{
		IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (service == null)
		{
			return;
		}
		DateTime? dateTime = null;
		if (beginDate is LuaTable luaTable)
		{
			dateTime = luaTable.ToDateTime();
		}
		if (dateTime.HasValue)
		{
			DateTime? endDate2 = null;
			if (endDate is LuaTable luaTable2)
			{
				endDate2 = luaTable2.ToDateTime();
			}
			int num = DateTime.Now.Hour;
			if (hours != null)
			{
				num = Convert.ToInt16(hours);
			}
			int num2 = DateTime.Now.Minute;
			if (minutes != null)
			{
				num2 = Convert.ToInt16(minutes);
			}
			int num3 = 1;
			if (daysInterval != null)
			{
				num3 = Convert.ToInt16(daysInterval);
			}
			int? minutesInterval2 = null;
			if (minutesInterval != null)
			{
				minutesInterval2 = Convert.ToInt32(minutesInterval);
			}
			service.AddDailyTrigger(name, dateTime.Value, endDate2, (short)num, (short)num2, (short)num3, minutesInterval2);
		}
	}

	[KernelFunction(Name = "kernel.win32addonsystemstartuptrigger")]
	internal static void AddSystemStartupTrigger(string name, object beginDate, object endDate)
	{
		IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
		if (service == null)
		{
			return;
		}
		DateTime? dateTime = null;
		if (beginDate is LuaTable luaTable)
		{
			dateTime = luaTable.ToDateTime();
		}
		if (dateTime.HasValue)
		{
			DateTime? endDate2 = null;
			if (endDate is LuaTable luaTable2)
			{
				endDate2 = luaTable2.ToDateTime();
			}
			service.AddSystemStartupTrigger(name, dateTime.Value, endDate2);
		}
	}
}
