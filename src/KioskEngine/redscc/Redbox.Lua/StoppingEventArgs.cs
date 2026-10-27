using System;

namespace Redbox.Lua;

public class StoppingEventArgs : EventArgs
{
	public int Line { get; private set; }

	public string FileName { get; private set; }

	public DebuggerActions Action { get; private set; }

	public LuaDebugBreakpoint Breakpoint { get; private set; }

	public StoppingEventArgs(string fileName, int line, DebuggerActions action, LuaDebugBreakpoint breakpoint)
	{
		Line = line;
		Action = action;
		FileName = fileName;
		Breakpoint = breakpoint;
	}
}
