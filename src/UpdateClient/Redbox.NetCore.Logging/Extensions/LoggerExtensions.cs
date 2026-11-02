using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Redbox.NetCore.Logging.Extensions;

public static class LoggerExtensions
{
	public static void LogDebugWithSource(this ILogger logger, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
	{
		logger.LogDebug(callerLocation.ClassName() + "." + memberName + " -> " + message);
	}

	public static void LogInfoWithSource(this ILogger logger, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
	{
		logger.LogInformation(callerLocation.ClassName() + "." + memberName + " -> " + message);
	}

	public static void LogErrorWithSource(this ILogger logger, Exception ex, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
	{
		logger.LogError(ex, callerLocation.ClassName() + "." + memberName + " -> " + message);
	}

	public static void LogErrorWithSource(this ILogger logger, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
	{
		logger.LogError(callerLocation.ClassName() + "." + memberName + " -> " + message);
	}

	public static void LogWarningWithSource(this ILogger logger, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
	{
		logger.LogWarning(callerLocation.ClassName() + "." + memberName + " -> " + message);
	}

	public static void LogCriticalWithSource(this ILogger logger, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
	{
		logger.LogCritical(callerLocation.ClassName() + "." + memberName + " -> " + message);
	}

	public static void LogCriticalWithSource(this ILogger logger, Exception ex, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
	{
		logger.LogCritical(ex, callerLocation.ClassName() + "." + memberName + " -> " + message);
	}

	private static string? ClassName(this string callerLocation)
	{
		if (!callerLocation.Contains("\\"))
		{
			string? unixFileName = callerLocation.Split('/').LastOrDefault();
			if (unixFileName == null)
			{
				return null;
			}
			return unixFileName.TrimEnd(".cs");
		}
		string? fileName = callerLocation.Split("\\").LastOrDefault();
		if (fileName == null)
		{
			return null;
		}
		return fileName.TrimEnd(".cs");
	}

	private static string? TrimEnd(this string? value, string substring)
	{
		if (value == null || !value.EndsWith(substring))
		{
			return value;
		}
		return value.Remove(value.LastIndexOf(substring));
	}
}
