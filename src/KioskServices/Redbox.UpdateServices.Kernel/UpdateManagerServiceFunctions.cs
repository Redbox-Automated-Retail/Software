using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.UpdateServices.Proxy.ComponentModel;

namespace Redbox.UpdateServices.Kernel;

public static class UpdateManagerServiceFunctions
{
	[KernelFunction(Name = "kernel.updmgrsvc_forcesubscriptionpublish")]
	internal static LuaTable? ForceSubscriptionPublish()
	{
		IUpdateManagerService updateManagerService = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (updateManagerService == null)
		{
			return null;
		}
		return CreateResultTable(updateManagerService.ForceSubscriptionPublish());
	}

	[KernelFunction(Name = "kernel.updmgrsvc_forcescheduleevent")]
	internal static LuaTable? ForceScheduleEvent(string name)
	{
		IUpdateManagerService updateManagerService = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (updateManagerService == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(updateManagerService.ForceScheduleEvent(name, out var isRunning));
		luaTable["is_running"] = isRunning;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.updmgrsvc_getsubscriptionstate")]
	internal static LuaTable? GetSubscriptionState()
	{
		IUpdateManagerService updateManagerService = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (updateManagerService == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(updateManagerService.GetSubscriptionState(out var lastRun, out var subscriptionState));
		luaTable["last_run"] = lastRun.ToString();
		luaTable["state"] = subscriptionState;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.updmgrsvc_executecommand")]
	internal static LuaTable? ExecuteCommand(string command)
	{
		IUpdateManagerService updateManagerService = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (updateManagerService == null)
		{
			return null;
		}
		LuaTable resultTable = CreateResultTable(updateManagerService.ExecuteCommand(command, out var messages));
		LuaTable messageTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		int index = 1;
		foreach (string message in messages)
		{
			messageTable[index++] = message;
		}
		resultTable["messages"] = messageTable;
		return resultTable;
	}

	[KernelFunction(Name = "kernel.updmgrsvc_getdownloads")]
	internal static LuaTable? GetDownloads()
	{
		IUpdateManagerService updateManagerService = ServiceLocator.Instance.GetService<IUpdateManagerService>();
		if (updateManagerService == null)
		{
			return null;
		}
		LuaTable resultTable = CreateResultTable(updateManagerService.GetDownloadState(out var downloads));
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable downloadsTable = (LuaTable)kernelService.NewTable();
		int index = 1;
		foreach (IDownloadData download in downloads)
		{
			LuaTable downloadTable = (LuaTable)kernelService.NewTable();
			downloadTable["id"] = download.Id.ToString();
			downloadTable["name"] = download.Name;
			downloadTable["owner"] = download.Owner;
			downloadTable["status"] = download.Status;
			downloadTable["job_type"] = download.JobType;
			downloadTable["total_bytes"] = download.TotalBytes;
			downloadTable["start_time"] = download.StartTime.ToString();
			downloadTable["finish_time"] = (download.FinishTime.HasValue ? download.FinishTime.ToString() : null);
			downloadTable["modified_time"] = download.ModifiedTime.ToString();
			downloadTable["total_bytes_transferred"] = download.TotalBytesTransferred;
			downloadsTable[index++] = downloadTable;
		}
		resultTable["downloads"] = downloadsTable;
		return resultTable;
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable resultTable = (LuaTable)kernelService.NewTable();
		resultTable["success"] = !errors.ContainsError();
		LuaTable errorTable = (LuaTable)kernelService.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			errorTable[error.Code] = error.Description;
		}
		resultTable["errors"] = errorTable;
		return resultTable;
	}
}
