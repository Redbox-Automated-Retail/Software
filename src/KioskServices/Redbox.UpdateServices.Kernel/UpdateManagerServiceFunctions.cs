using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.UpdateServices.Proxy.ComponentModel;

namespace Redbox.UpdateServices.Kernel;

public static class UpdateManagerServiceFunctions
{
	[KernelFunction(Name = "kernel.updmgrsvc_forcesubscriptionpublish")]
	internal static LuaTable ForceSubscriptionPublish()
	{
		IUpdateManagerService service = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(service.ForceSubscriptionPublish());
	}

	[KernelFunction(Name = "kernel.updmgrsvc_forcescheduleevent")]
	internal static LuaTable ForceScheduleEvent(string name)
	{
		IUpdateManagerService service = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (service == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(service.ForceScheduleEvent(name, out var isRunning));
		luaTable["is_running"] = isRunning;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.updmgrsvc_getsubscriptionstate")]
	internal static LuaTable GetSubscriptionState()
	{
		IUpdateManagerService service = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (service == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(service.GetSubscriptionState(out var lastRun, out var subscriptionState));
		luaTable["last_run"] = lastRun.ToString();
		luaTable["state"] = subscriptionState;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.updmgrsvc_executecommand")]
	internal static LuaTable ExecuteCommand(string command)
	{
		IUpdateManagerService service = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (service == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(service.ExecuteCommand(command, out var messages));
		LuaTable luaTable2 = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		int num = 1;
		foreach (string item in messages)
		{
			luaTable2[num++] = item;
		}
		luaTable["messages"] = luaTable2;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.updmgrsvc_getdownloads")]
	internal static LuaTable GetDownloads()
	{
		IUpdateManagerService service = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (service == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(service.GetDownloadState(out var downloads));
		IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable2 = (LuaTable)service2.NewTable();
		int num = 1;
		foreach (IDownloadData item in downloads)
		{
			LuaTable luaTable3 = (LuaTable)service2.NewTable();
			luaTable3["id"] = item.Id.ToString();
			luaTable3["name"] = item.Name;
			luaTable3["owner"] = item.Owner;
			luaTable3["status"] = item.Status;
			luaTable3["job_type"] = item.JobType;
			luaTable3["total_bytes"] = item.TotalBytes;
			luaTable3["start_time"] = item.StartTime.ToString();
			luaTable3["finish_time"] = (item.FinishTime.HasValue ? item.FinishTime.ToString() : null);
			luaTable3["modified_time"] = item.ModifiedTime.ToString();
			luaTable3["total_bytes_transferred"] = item.TotalBytesTransferred;
			luaTable2[num++] = luaTable3;
		}
		luaTable["downloads"] = luaTable2;
		return luaTable;
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)service.NewTable();
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = (LuaTable)service.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
