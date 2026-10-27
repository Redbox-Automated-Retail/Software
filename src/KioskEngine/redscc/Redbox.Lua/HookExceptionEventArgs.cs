using System;

namespace Redbox.Lua;

public class HookExceptionEventArgs : EventArgs
{
	public Exception Exception { get; private set; }

	public HookExceptionEventArgs(Exception ex)
	{
		Exception = ex;
	}
}
