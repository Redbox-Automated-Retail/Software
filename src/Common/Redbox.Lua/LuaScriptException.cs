using System;

namespace Redbox.Lua;

public class LuaScriptException : LuaException
{
	private readonly string m_source;

	public bool IsNetException { get; private set; }

	public override string Source => m_source;

	public LuaScriptException(string message, string source)
		: base(message)
	{
		m_source = source;
	}

	public LuaScriptException(Exception innerException, string source)
		: base("A .NET exception occured in user-code", innerException)
	{
		m_source = source;
		IsNetException = true;
	}

	public override string ToString()
	{
		return GetType().FullName + ": " + m_source + Message;
	}
}
