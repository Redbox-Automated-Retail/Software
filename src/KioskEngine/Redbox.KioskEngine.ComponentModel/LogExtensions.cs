using System;
using System.Runtime.CompilerServices;
using System.Text;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel;

public static class LogExtensions
{
	public static void LogEntireException(this LogHelper log, string message, Exception e, [CallerMemberName] string memberName = null)
	{
		Type type = e.GetType();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Exception Details - Type: {type} Caller Member Name: {memberName}");
		stringBuilder.AppendLine(" Message: " + message);
		while (e != null)
		{
			stringBuilder.AppendLine("Exception Message " + e.Message);
			if (e.StackTrace != null)
			{
				stringBuilder.AppendLine("Stack Trace");
				stringBuilder.AppendLine(e.StackTrace);
			}
			e = e.InnerException;
		}
		log.Log(stringBuilder.ToString());
	}
}
