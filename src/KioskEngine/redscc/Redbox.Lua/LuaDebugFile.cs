namespace Redbox.Lua;

public class LuaDebugFile
{
	private readonly LuaDebugBreakpointContainer m_breakpoints = new LuaDebugBreakpointContainer();

	public string FileName { get; private set; }

	public LuaDebugger Debugger { get; private set; }

	public LuaDebugBreakpointContainer Breakpoints => m_breakpoints;

	public LuaDebugFile(LuaDebugger debugger, string fileName)
	{
		Debugger = debugger;
		FileName = fileName;
	}

	public LuaDebugBreakpoint AddBreakpoint(int line)
	{
		LuaDebugBreakpoint luaDebugBreakpoint = GetBreakpoint(line);
		if (luaDebugBreakpoint != null)
		{
			luaDebugBreakpoint.Enabled = true;
		}
		else
		{
			luaDebugBreakpoint = Debugger.BreakpointFactory.CreateBreakpoint(this, line);
			m_breakpoints.Add(luaDebugBreakpoint);
		}
		return luaDebugBreakpoint;
	}

	public void RemoveBreakpoint(int line)
	{
		RemoveBreakpoint(GetBreakpoint(line));
	}

	public void RemoveBreakpoint(LuaDebugBreakpoint breakpoint)
	{
		m_breakpoints.Remove(breakpoint);
	}

	public LuaDebugBreakpoint GetBreakpoint(int line)
	{
		foreach (LuaDebugBreakpoint breakpoint in m_breakpoints)
		{
			if (breakpoint.Line == line)
			{
				return breakpoint;
			}
		}
		return null;
	}

	public LuaDebugBreakpoint ToggleBreakpoint(int line)
	{
		LuaDebugBreakpoint breakpoint = GetBreakpoint(line);
		if (breakpoint == null)
		{
			return AddBreakpoint(line);
		}
		RemoveBreakpoint(breakpoint);
		return null;
	}
}
