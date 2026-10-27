using System;

namespace KioskClientService.API.Vistar;

public static class DateTimeExtensions
{
	public static DateTime GetLocalFromUtcUnixTime(this DateTime dateTime, long unixTime)
	{
		return DateTime.UnixEpoch.AddSeconds(unixTime).ToLocalTime();
	}

	public static DateTime StripSeconds(this DateTime dateTime)
	{
		return dateTime.AddSeconds(-dateTime.Second);
	}

	public static long ToUtcUnixTime(this DateTime dateTime)
	{
		if (dateTime.Kind != DateTimeKind.Utc)
		{
			dateTime = dateTime.ToUniversalTime();
		}
		return (long)Math.Floor((dateTime - DateTime.UnixEpoch).TotalSeconds);
	}
}
