using System;

namespace Redbox.HAL.Script.Framework;

internal class ScriptMessage
{
	internal readonly string Message;

	internal readonly DateTime EventTime;

	public override string ToString()
	{
		return $"{EventTime}|{Message}";
	}

	internal static ScriptMessage Parse(string message)
	{
		string[] array = message.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		return new ScriptMessage(DateTime.Parse(array[0]), array[1]);
	}

	internal ScriptMessage(DateTime time, string msg)
	{
		EventTime = time;
		Message = msg;
	}
}
