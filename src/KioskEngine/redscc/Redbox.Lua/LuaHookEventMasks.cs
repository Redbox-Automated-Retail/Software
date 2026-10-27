using System;

namespace Redbox.Lua;

[Flags]
public enum LuaHookEventMasks
{
	LuaMaskCall = 1,
	LuaMaskRet = 2,
	LuaMaskLine = 4,
	LuaMaskCount = 8,
	LuaMaskAll = int.MaxValue
}
