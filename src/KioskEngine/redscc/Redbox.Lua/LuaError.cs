using System;

namespace Redbox.Lua;

public class LuaError
{
	public int? LineNumber { get; internal set; }

	public string Message { get; internal set; }

	public string ChunkName { get; internal set; }

	public string RawMessage { get; internal set; }

	public static LuaError FromString(string rawMessage)
	{
		string[] array = rawMessage.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		int? lineNumber = null;
		string message = null;
		string chunkName = null;
		if (array.Length == 3)
		{
			int num = array[0].IndexOf('"');
			if (num != -1)
			{
				int num2 = array[0].LastIndexOf('"');
				if (num2 != -1)
				{
					chunkName = array[0].Substring(num + 1, num2 - num - 1);
				}
			}
			if (int.TryParse(array[1], out var result))
			{
				lineNumber = result;
			}
			message = array[2];
		}
		LuaError luaError = new LuaError();
		luaError.Message = message;
		luaError.ChunkName = chunkName;
		luaError.RawMessage = rawMessage;
		luaError.LineNumber = lineNumber;
		return luaError;
	}

	private LuaError()
	{
	}
}
