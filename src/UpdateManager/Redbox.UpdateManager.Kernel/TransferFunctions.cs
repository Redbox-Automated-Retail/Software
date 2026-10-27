using System;
using System.Net;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class TransferFunctions
{
	[KernelFunction(Name = "kernel.transferarejobsrunning")]
	internal static LuaTable AreJobRunning()
	{
		LuaTable luaTable = CreateResultTable(ServiceLocator.Instance.GetService<ITransferService>().AreJobsRunning(out var isRunning));
		luaTable["isrunning"] = isRunning;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.transferjobsetpriority")]
	internal static LuaTable SetPriority(string id, string priority)
	{
		ErrorList errorList = new ErrorList();
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		errorList.AddRange(service.GetJob(new Guid(id), out var job));
		if (errorList.ContainsError())
		{
			return CreateResultTable(errorList);
		}
		switch (priority)
		{
		case "high":
			errorList.AddRange(job.SetPriority(TransferJobPriority.High));
			break;
		case "foreground":
			errorList.AddRange(job.SetPriority(TransferJobPriority.Foreground));
			break;
		case "normal":
			errorList.AddRange(job.SetPriority(TransferJobPriority.Normal));
			break;
		case "low":
			errorList.AddRange(job.SetPriority(TransferJobPriority.Foreground));
			break;
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.resettransfers")]
	internal static LuaTable Reset()
	{
		return CreateResultTable(ServiceLocator.Instance.GetService<ITransferService>().CancelAll());
	}

	[KernelFunction(Name = "kernel.transfergetrepositoriesintransit")]
	internal static LuaTable GetRevisionsInTransit()
	{
		ErrorList repositoriesInTransit = ServiceLocator.Instance.GetService<ITransferService>().GetRepositoriesInTransit(out var inTransit);
		if (repositoriesInTransit.ContainsError())
		{
			repositoriesInTransit.ForEach(delegate
			{
				LogHelper.Instance.Log("{0} Details: {1}");
			});
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		int num = 1;
		foreach (string item in inTransit)
		{
			luaTable[num] = item;
			num++;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.checkconnectivity")]
	internal static bool CheckConnectivity(string url, int timeout)
	{
		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Timeout = timeout * 1000;
		WebResponse response;
		try
		{
			response = webRequest.GetResponse();
		}
		catch
		{
			return false;
		}
		return response != null;
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
