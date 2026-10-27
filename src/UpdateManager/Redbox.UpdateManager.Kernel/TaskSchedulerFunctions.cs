using System;
using System.Security.Cryptography;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class TaskSchedulerFunctions
{
	[KernelFunction(Name = "kernel.getrandomnumber")]
	internal static string GetRandomNumber(int min, int max)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[4];
		rNGCryptoServiceProvider.GetBytes(array);
		int num = BitConverter.ToInt32(array, 0);
		return (min + Math.Abs(num % (max - min))).ToString();
	}

	[KernelFunction(Name = "kernel.schedulecronjob")]
	internal static LuaTable ScheduleCronJob(string name, string payload, string payloadType, int startOffset, string cronExpression)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().ScheduleCronJob(name, payload, payloadType, (startOffset > 0) ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : ((TimeSpan?)null), null, null, cronExpression);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulecronscript")]
	internal static LuaTable ScheduleCronScript(string name, string script, int startOffset, string cronExpression)
	{
		return ScheduleCronJob(name, script, "script", startOffset, cronExpression);
	}

	[KernelFunction(Name = "kernel.schedulecronwork")]
	internal static LuaTable ScheduleCronWork(int startOffset, string cronExpression)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime);
	}

	[KernelFunction(Name = "kernel.schedulesimplejob")]
	internal static LuaTable ScheduleSimpleJob(string name, string payload, string payloadTypeString, int startOffset, string repeatIntervalString)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		if (!TimeSpan.TryParse(repeatIntervalString, out var result))
		{
			return CreateResultTable(new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("I001", "Could not parse time interval", "Please use a parsable time interval") });
		}
		ErrorList errorList = service.ScheduleSimpleJob(name, payload, payloadTypeString, (startOffset > 0) ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : ((TimeSpan?)null), null, null, result);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.scheduleonetimescript")]
	internal static LuaTable ScheduleSimpleJob(string name, string script, int startOffset, string timeString)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		if (!TimeSpan.TryParse(timeString, out var result))
		{
			return CreateResultTable(new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("I001", "Could not parse time interval", "Please use a parsable time interval") });
		}
		ErrorList errorList = service.ScheduleSimpleJob(name, script, "script", (startOffset > 0) ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : ((TimeSpan?)null), DateTime.MinValue.Add(result), null, TimeSpan.Zero);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulesimplescript")]
	internal static LuaTable ScheduleSimpleScript(string name, string payload, int startOffset, string repeatIntervalString)
	{
		return ScheduleSimpleJob(name, payload, "script", startOffset, repeatIntervalString);
	}

	[KernelFunction(Name = "kernel.schedulesimplerandomwork")]
	internal static LuaTable ScheduleSimpleRandomWork(int startOffset, string repeatIntervalString)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime);
	}

	[KernelFunction(Name = "kernel.scheduleeventlist")]
	internal static LuaTable List()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().List(out var tasks);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		int num = 1;
		foreach (ITask item in tasks)
		{
			luaTable[num] = $"{item.Name} {item.Payload}";
			num++;
		}
		LuaTable luaTable2 = CreateResultTable(errorList);
		luaTable2["events"] = luaTable;
		return luaTable2;
	}

	[KernelFunction(Name = "kernel.scheduleeventdelete")]
	internal static LuaTable Delete(string name)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().Delete(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.eraseschedule")]
	internal static void Erase()
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		if (service == null)
		{
			return;
		}
		ErrorList errorList = service.Clear();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
	}

	[KernelFunction(Name = "kernel.eraseschedulenodefault")]
	internal static LuaTable EraseNoDefault()
	{
		try
		{
			ErrorList errorList = new ErrorList();
			ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
			errorList.AddRange(service.List(out var tasks));
			foreach (ITask item in tasks)
			{
				errorList.AddRange(service.Delete(item.Name));
			}
			if (errorList.ContainsError())
			{
				errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error error)
				{
					LogHelper.Instance.Log("{0} Details: {1}", error, error.Details);
				});
			}
			return CreateResultTable(errorList);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TaskSchedulerFunctions.EraseNoDefault - unhandled exception", e);
			return CreateResultTable(new ErrorList());
		}
	}

	[KernelFunction(Name = "kernel.schedulerstop")]
	internal static LuaTable Stop()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().Stop();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulerstart")]
	internal static LuaTable Start()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().Start();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulerrestart")]
	internal static LuaTable Restart()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().Restart();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (Redbox.UpdateManager.ComponentModel.Error error in errors)
		{
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
