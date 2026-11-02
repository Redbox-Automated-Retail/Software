using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class EventNotificationFunctions
{
	[KernelFunction(Name = "kernel.notifyaddevent")]
	internal static LuaTable? AddEvent(string name, string description)
	{
		IEventNotifyService eventNotifyService = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (eventNotifyService != null)
		{
			return CreateResultTable(eventNotifyService.AddEvent(name, description));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyeventstart")]
	internal static LuaTable? EventStart(string name)
	{
		IEventNotifyService eventNotifyService = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (eventNotifyService != null)
		{
			return CreateResultTable(eventNotifyService.EventStart(name));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyeventerror")]
	internal static LuaTable? EventError(string name, string code, string description, string details)
	{
		IEventNotifyService eventNotifyService = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (eventNotifyService != null)
		{
			return CreateResultTable(eventNotifyService.EventErrored(name, code, description, details));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyeventcomplete")]
	internal static LuaTable? EventComplete(string name)
	{
		IEventNotifyService eventNotifyService = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (eventNotifyService != null)
		{
			return CreateResultTable(eventNotifyService.EventComplete(name));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyexit")]
	internal static LuaTable? Exit()
	{
		IEventNotifyService eventNotifyService = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (eventNotifyService != null)
		{
			return CreateResultTable(eventNotifyService.Exit());
		}
		return null;
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = !errors.ContainsError();
		LuaTable errorTable = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (Redbox.UpdateManager.ComponentModel.Error error in errors)
		{
			errorTable[error.Code] = error.Description;
		}
		luaTable["errors"] = errorTable;
		return luaTable;
	}
}
