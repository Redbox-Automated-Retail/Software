using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class LogFunctions
{
	[KernelFunction(Name = "kernel.log")]
	internal static void Log(string message)
	{
		LogHelper.Instance.Log($"Lua Runtime| {message}");
	}

	[KernelFunction(Name = "kernel.logto")]
	internal static void LogTo(string logName, string message)
	{
		LogHelper.Instance.LogTo(logName, $"Lua Runtime| {message}");
	}

	[KernelFunction(Name = "kernel.logf")]
	internal static void Log(string message, object parms)
	{
		if (parms is LuaTable luaTable)
		{
			List<object> list = new List<object>();
			ExpandTableList(luaTable, list);
			LogHelper.Instance.Log($"Lua Runtime| {message}", list.ToArray());
			luaTable.Dispose();
		}
		else
		{
			Log(message);
		}
	}

	private static void ExpandTableList(LuaTable table, ICollection<object> list)
	{
		foreach (object value in table.Values)
		{
			if (value is LuaTable)
			{
				ExpandTableList((LuaTable)value, list);
			}
			list.Add(value.ToString());
		}
	}

	private static void LogToOutputWindow(string message)
	{
		if (ServiceLocator.Instance.GetService<IIdeService>() != null)
		{
			IdeMainForm.LogToOutputWindow(message);
		}
	}
}
