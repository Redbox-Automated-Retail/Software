namespace Redbox.Lua;

public interface ILuaDebugFileFactory
{
	LuaDebugFile CreateFile(LuaDebugger debugger, string fileName);
}
