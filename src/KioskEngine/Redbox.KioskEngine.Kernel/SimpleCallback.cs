using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal sealed class SimpleCallback : ICallbackEntry, IDisposable
{
	public LuaFunction Function { get; set; }

	public object[] Parameters { get; set; }

	public bool EnableDebugger { get; set; }

	public string Name { get; set; } = "SimpleCallback";

	public void Dispose()
	{
		if (Parameters != null)
		{
			object[] parameters = Parameters;
			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i] is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}
		if (Function != null)
		{
			Function.Dispose();
		}
	}

	public void Invoke()
	{
		if (Function == null)
		{
			return;
		}
		try
		{
			Function.Call(Parameters);
		}
		catch (LuaException e)
		{
			if (EnableDebugger)
			{
				e.RaiseDebuggerOrError("SimpleCallback");
			}
			else
			{
				LogHelper.Instance.Log("An unhandled Lua exception was raised, debugger is not enabled.", e);
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in SimpleCallback handler.", e2);
		}
	}
}
