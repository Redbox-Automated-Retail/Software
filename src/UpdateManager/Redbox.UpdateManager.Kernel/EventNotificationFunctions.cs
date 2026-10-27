using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class EventNotificationFunctions
{
	[KernelFunction(Name = "kernel.notifyaddevent")]
	internal static LuaTable AddEvent(string name, string description)
	{
		IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (service != null)
		{
			return CreateResultTable(service.AddEvent(name, description));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyeventstart")]
	internal static LuaTable EventStart(string name)
	{
		IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (service != null)
		{
			return CreateResultTable(service.EventStart(name));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyeventerror")]
	internal static LuaTable EventError(string name, string code, string description, string details)
	{
		IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (service != null)
		{
			return CreateResultTable(service.EventErrored(name, code, description, details));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyeventcomplete")]
	internal static LuaTable EventComplete(string name)
	{
		IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (service != null)
		{
			return CreateResultTable(service.EventComplete(name));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.notifyexit")]
	internal static LuaTable Exit()
	{
		IEventNotifyService service = ServiceLocator.Instance.GetService<IEventNotifyService>();
		if (service != null)
		{
			return CreateResultTable(service.Exit());
		}
		return null;
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
