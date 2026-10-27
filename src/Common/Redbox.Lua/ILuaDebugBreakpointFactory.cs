namespace Redbox.Lua;

public interface ILuaDebugBreakpointFactory
{
	LuaDebugBreakpoint CreateBreakpoint(LuaDebugFile file, int line);
}
