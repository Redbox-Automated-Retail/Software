using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

public static class LoggerExtensions
{
	public static void LogInfoWithSource<T>(this ILogger<T> logger, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogInformation(name + "." + memberName + " -> " + message);
	}

	public static void LogWarningWithSource<T>(this ILogger<T> logger, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogWarning(name + "." + memberName + " -> " + message);
	}

	public static void LogDebugWithSource<T>(this ILogger<T> logger, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogDebug(name + "." + memberName + " -> " + message);
	}

	public static void LogErrorWithSource<T>(this ILogger<T> logger, Exception ex, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogError(ex, name + "." + memberName + " -> " + message);
	}

	public static void LogErrorWithSource<T>(this ILogger<T> logger, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogError(name + "." + memberName + " -> " + message);
	}

	public static void LogCriticalWithSource<T>(this ILogger<T> logger, Exception ex, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogCritical(ex, name + "." + memberName + " -> " + message);
	}
}
