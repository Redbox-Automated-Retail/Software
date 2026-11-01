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
			int firstQuote = parts[0].IndexOf('"');
			if (firstQuote != -1)
			{
				int lastQuote = parts[0].LastIndexOf('"');
				if (lastQuote != -1)
				{
					chunkName = parts[0].Substring(firstQuote + 1, lastQuote - firstQuote - 1);
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
