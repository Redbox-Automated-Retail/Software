using System.Runtime.InteropServices;

namespace Redbox.Lua;

public class LuaDebugInfo
{
	public string Name { get; internal set; }

	public string What { get; internal set; }

	public string Source { get; internal set; }

	public string NameWhat { get; internal set; }

	public int CurrentLine { get; internal set; }

	public int LineDefined { get; internal set; }

	public string ShortSource { get; internal set; }

	public int ActiveFunction { get; internal set; }

	public LuaDebug LuaDebug { get; internal set; }

	public int NumberUpValues { get; internal set; }

	public int LastLineDefined { get; internal set; }

	public LuaHookEventCodes EventCode { get; internal set; }

	internal LuaDebugInfo(LuaDebug luaDebug)
	{
		LuaDebug = luaDebug;
		EventCode = luaDebug.eventCode;
		NumberUpValues = luaDebug.nups;
		LastLineDefined = luaDebug.lastlinedefined;
		CurrentLine = LuaDebug.currentline;
		LineDefined = LuaDebug.linedefined;
		ShortSource = LuaDebug.shortsrc;
		ActiveFunction = LuaDebug.i_ci;
		Name = Marshal.PtrToStringAnsi(LuaDebug.name);
		What = Marshal.PtrToStringAnsi(LuaDebug.what);
		Source = Marshal.PtrToStringAnsi(LuaDebug.source);
		NameWhat = Marshal.PtrToStringAnsi(LuaDebug.namewhat);
	}
}
