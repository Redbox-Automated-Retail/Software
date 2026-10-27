using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

public static class LuaExceptionExtensions
{
	public static void RaiseDebuggerOrError(this LuaException e, string resourceName)
	{
		LuaScriptException ex = e as LuaScriptException;
		string rawMessage = e.Message;
		if (ex != null && ex.IsNetException)
		{
			rawMessage = ex.Source;
		}
		LuaError luaError = LuaError.FromString(rawMessage);
		LogHelper.Instance.Log($"An unhandled error was raised from script resource '{luaError.ChunkName ?? resourceName}' at line {luaError.LineNumber.GetValueOrDefault()}: {luaError.Message ?? luaError.RawMessage}", LogEntryType.Error);
		LogException(e);
		IDebugService service = ServiceLocator.Instance.GetService<IDebugService>();
		if (!service.IsEnabled)
		{
			LogHelper.Instance.Log("IDebugService.IsEnabled == false; not opening debugger window.");
			return;
		}
		LogHelper.Instance.Log("IDebugService.IsEnabled == true; opening debugger window.");
		if (!luaError.LineNumber.HasValue)
		{
			service.ActivateDebugger(luaError.ChunkName ?? resourceName);
			return;
		}
		string text = luaError.Message;
		if (ex != null && ex.IsNetException)
		{
			text += "A .NET exception occured.\n This location the script entered .net code.\n  See log file for details.\n";
			if (ex.InnerException != null)
			{
				text = text + "Exception:\n" + ex.InnerException.Message;
			}
		}
		service.ActivateDebugger(luaError.ChunkName ?? resourceName, luaError.LineNumber.Value, text);
	}

	private static void LogException(Exception exception)
	{
		if (exception != null)
		{
			LogHelper.Instance.Log("Message: {0}", exception.Message);
			LogHelper.Instance.Log("Source: {0}", exception.Source);
			LogHelper.Instance.Log("Stack: {0}", exception.StackTrace);
			LogException(exception.InnerException);
		}
	}
}
