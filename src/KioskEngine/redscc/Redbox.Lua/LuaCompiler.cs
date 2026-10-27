namespace Redbox.Lua;

public class LuaCompiler
{
	private readonly LuaVirtualMachine m_virtualMachine;

	public string Version { get; private set; }

	public string Architecture { get; private set; }

	public LuaCompiler(LuaVirtualMachine virtualMachine)
	{
		m_virtualMachine = virtualMachine;
		if (m_virtualMachine["jit"] is LuaTable luaTable)
		{
			Version = luaTable["version"] as string;
			Architecture = luaTable["arch"] as string;
		}
		else
		{
			Version = "Not Installed";
			Architecture = "Not Installed";
		}
	}
}
