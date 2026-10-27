using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class CallbackFunctions
{
	[KernelFunction(Name = "kernel.callbackenqueue")]
	internal static void EnqueueCallback(LuaFunction func, object parms, string name = "SimpleCallback")
	{
		EnqueueSimpleCallback(func, parms, enableDebugger: true, name ?? "");
	}

	[KernelFunction(Name = "kernel.callbackenqueuenobreak")]
	internal static void EnqueueCallbackNoBreak(LuaFunction func, object parms)
	{
		EnqueueSimpleCallback(func, parms, enableDebugger: false, "callbackenqueuenobreak");
	}

	[KernelFunction(Name = "kernel.callbacksuspend")]
	internal static void SuspendCallbackQueue()
	{
		ServiceLocator.Instance.GetService<ICallbackService>()?.Suspend();
	}

	[KernelFunction(Name = "kernel.callbackresume")]
	internal static void ResumeCallbackQueue()
	{
		ServiceLocator.Instance.GetService<ICallbackService>()?.Resume();
	}

	[KernelFunction(Name = "kernel.callbackflush")]
	internal static void Flush()
	{
		ServiceLocator.Instance.GetService<ICallbackService>()?.Flush();
	}

	private static void EnqueueSimpleCallback(LuaFunction func, object parms, bool enableDebugger, string name)
	{
		if (func == null)
		{
			return;
		}
		List<object> list = new List<object>();
		if (parms is LuaTable luaTable)
		{
			foreach (object key in luaTable.Keys)
			{
				list.Add(luaTable[key]);
			}
			luaTable.Dispose();
		}
		ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new SimpleCallback
		{
			Function = func,
			EnableDebugger = enableDebugger,
			Parameters = list.ToArray(),
			Name = name
		});
	}
}
