using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class SchedulerFunctions
{
	[KernelFunction(Name = "scheduler_services.start")]
	internal static void Start()
	{
		ServiceLocator.Instance.GetService<ISchedulerService>()?.Start();
	}

	[KernelFunction(Name = "scheduler_services.shutdown")]
	internal static void Shutdown()
	{
		ServiceLocator.Instance.GetService<ISchedulerService>()?.Shutdown();
	}

	[KernelFunction(Name = "scheduler_services.clear")]
	internal static void Clear()
	{
		ServiceLocator.Instance.GetService<ISchedulerService>()?.Clear();
	}

	[KernelFunction(Name = "scheduler_services.remove_entry")]
	internal static bool RemoveScheduleEntry(string jobname)
	{
		return ServiceLocator.Instance.GetService<ISchedulerService>()?.RemoveScheduleEntry(jobname) ?? false;
	}

	[KernelFunction(Name = "scheduler_services.add_entry")]
	internal static void AddScheduleEntry(string jobname, string label, object startTime, object endTime, string cronExpression, string functionName, string programName, string misfireInstruction, object parameters)
	{
		ISchedulerService service = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (service == null)
		{
			return;
		}
		DateTime? dateTime = null;
		DateTime? endTime2 = null;
		if (startTime is LuaTable luaTable)
		{
			dateTime = luaTable.ToDateTime();
		}
		if (endTime is LuaTable luaTable2)
		{
			endTime2 = luaTable2.ToDateTime();
		}
		List<object> list = new List<object>();
		if (parameters != null)
		{
			if (parameters is LuaTable luaTable3)
			{
				foreach (object key in luaTable3.Keys)
				{
					list.Add(luaTable3[key]);
				}
			}
			else
			{
				list.Add(parameters);
			}
		}
		service.AddScheduleEntry(jobname, label, dateTime ?? DateTime.UtcNow, endTime2, cronExpression, functionName, programName, misfireInstruction, list.ToArray());
	}

	[KernelFunction(Name = "scheduler_services.get_entries")]
	internal static LuaTable GetScheduleEntries()
	{
		if (ServiceLocator.Instance.GetService<IKernelService>() == null)
		{
			return null;
		}
		ISchedulerService service = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (service == null)
		{
			return null;
		}
		int num = 1;
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (ISchedulerEntry scheduleEntry in service.GetScheduleEntries())
		{
			new ErrorList();
			CreateResultTable(scheduleEntry.JobName)["results"] = luaTable;
			LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
			luaTable2["id"] = scheduleEntry.JobName;
			luaTable2["status"] = "Pending";
			luaTable2["label"] = scheduleEntry.Label;
			luaTable2["program"] = scheduleEntry.Program;
			luaTable2["function"] = scheduleEntry.FunctionName;
			luaTable2["starttime"] = scheduleEntry.CronExpression;
			luaTable2["executiontime"] = string.Empty;
			luaTable2["priority"] = string.Empty;
			luaTable2["misfireinstruction"] = scheduleEntry.MisfireInstruction;
			luaTable[num++] = luaTable2;
		}
		return luaTable;
	}

	[KernelFunction(Name = "scheduler_services.get_entry")]
	internal static LuaTable GetScheduleEntry(string jobname)
	{
		if (ServiceLocator.Instance.GetService<IKernelService>() == null)
		{
			return null;
		}
		ISchedulerService service = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (service == null)
		{
			return null;
		}
		ISchedulerEntry scheduleEntry = service.GetScheduleEntry(jobname);
		if (scheduleEntry == null)
		{
			return null;
		}
		return new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["id"] = scheduleEntry.JobName,
			["status"] = "Pending",
			["label"] = scheduleEntry.Label,
			["program"] = scheduleEntry.Program,
			["function"] = scheduleEntry.FunctionName,
			["starttime"] = scheduleEntry.CronExpression,
			["executiontime"] = string.Empty,
			["priority"] = string.Empty,
			["misfireinstruction"] = scheduleEntry.MisfireInstruction
		};
	}

	private static LuaTable CreateResultTable(string jobname)
	{
		LuaTable obj = new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["success"] = true,
			["job_id"] = jobname
		};
		LuaTable value = new LuaTable(KernelService.Instance.LuaRuntime);
		obj["errors"] = value;
		return obj;
	}
}
