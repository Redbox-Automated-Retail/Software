namespace Redbox.Lua;

public class CallStackEntry
{
	public LuaDebugInfo DebugInfo { get; private set; }

	public string FunctionName => DebugInfo.Name;

	public string FileName => DebugInfo.Source;

	public int Line => DebugInfo.CurrentLine;

	internal CallStackEntry(LuaDebugInfo luaDebugInfo)
	{
		DebugInfo = luaDebugInfo;
	}
}
