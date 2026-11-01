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
		ISchedulerService schedulerService = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (schedulerService == null)
		{
			return;
		}
		DateTime? startDateTime = null;
		DateTime? endDateTime = null;
		if (startTime is LuaTable startTable)
		{
			startDateTime = startTable.ToDateTime();
		}
		if (endTime is LuaTable endTable)
		{
			endDateTime = endTable.ToDateTime();
		}
		List<object> parameterValues = new List<object>();
		if (parameters != null)
		{
			if (parameters is LuaTable parameterTable)
			{
				foreach (object key in parameterTable.Keys)
				{
					parameterValues.Add(parameterTable[key]);
				}
			}
			else
			{
				parameterValues.Add(parameters);
			}
		}
		schedulerService.AddScheduleEntry(jobname, label, startDateTime ?? DateTime.UtcNow, endDateTime, cronExpression, functionName, programName, misfireInstruction, parameterValues.ToArray());
	}

	[KernelFunction(Name = "scheduler_services.get_entries")]
	internal static LuaTable GetScheduleEntries()
	{
		if (ServiceLocator.Instance.GetService<IKernelService>() == null)
		{
			return null;
		}
		ISchedulerService schedulerService = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (schedulerService == null)
		{
			return null;
		}
		int index = 1;
		LuaTable entries = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (ISchedulerEntry scheduleEntry in schedulerService.GetScheduleEntries())
		{
			new ErrorList();
			CreateResultTable(scheduleEntry.JobName)["results"] = entries;
			LuaTable entry = new LuaTable(KernelService.Instance.LuaRuntime);
			entry["id"] = scheduleEntry.JobName;
			entry["status"] = "Pending";
			entry["label"] = scheduleEntry.Label;
			entry["program"] = scheduleEntry.Program;
			entry["function"] = scheduleEntry.FunctionName;
			entry["starttime"] = scheduleEntry.CronExpression;
			entry["executiontime"] = string.Empty;
			entry["priority"] = string.Empty;
			entry["misfireinstruction"] = scheduleEntry.MisfireInstruction;
			entries[index++] = entry;
		}
		return entries;
	}

	[KernelFunction(Name = "scheduler_services.get_entry")]
	internal static LuaTable GetScheduleEntry(string jobname)
	{
		if (ServiceLocator.Instance.GetService<IKernelService>() == null)
		{
			return null;
		}
		ISchedulerService schedulerService = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (schedulerService == null)
		{
			return null;
		}
		ISchedulerEntry scheduleEntry = schedulerService.GetScheduleEntry(jobname);
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
		LuaTable resultTable = new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["success"] = true,
			["job_id"] = jobname
		};
		LuaTable errors = new LuaTable(KernelService.Instance.LuaRuntime);
		resultTable["errors"] = errors;
		return resultTable;
	}
}
