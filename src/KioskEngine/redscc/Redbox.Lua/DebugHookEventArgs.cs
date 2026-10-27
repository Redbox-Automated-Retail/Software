using System;

namespace Redbox.Lua;

public class DebugHookEventArgs : EventArgs
{
	public LuaDebugInfo DebugInfo { get; private set; }

	public DebugHookEventArgs(LuaDebugInfo luaDebugInfo)
	{
		DebugInfo = luaDebugInfo;
	}
}
