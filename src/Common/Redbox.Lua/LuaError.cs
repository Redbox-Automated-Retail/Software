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
		string[] parts = rawMessage.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		int? lineNumber = null;
		string message = null;
		string chunkName = null;
		if (parts.Length == 3)
		{
			int num = parts[0].IndexOf('"');
			if (num != -1)
			{
				int num2 = parts[0].LastIndexOf('"');
				if (num2 != -1)
				{
					chunkName = parts[0].Substring(num + 1, num2 - num - 1);
				}
			}
			if (int.TryParse(parts[1], out var result))
			{
				lineNumber = result;
			}
			message = parts[2];
		}
		return new LuaError
		{
			Message = message,
			ChunkName = chunkName,
			RawMessage = rawMessage,
			LineNumber = lineNumber
		};
	}

	private LuaError()
	{
	}
}
