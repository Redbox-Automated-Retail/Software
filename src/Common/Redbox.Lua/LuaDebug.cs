using System;
using System.Runtime.InteropServices;

namespace Redbox.Lua;

public struct LuaDebug
{
	public LuaHookEventCodes eventCode;

	public IntPtr name;

	public IntPtr namewhat;

	public IntPtr what;

	public IntPtr source;

	public int currentline;

	public int nups;

	public int linedefined;

	public int lastlinedefined;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)]
	public string shortsrc;

	public int i_ci;
}
