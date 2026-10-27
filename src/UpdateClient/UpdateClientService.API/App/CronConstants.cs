using System;

namespace UpdateClientService.API.App;

public class CronConstants
{
	public static readonly string AtRandomMinuteEvery12thHour = $"{new Random().Next(0, 59)} */12 * * *";

	public const string AtMinute0Every12thHour = "0 */12 * * *";

	public static string EveryXHours(int hours)
	{
		return $"0 */{hours} * * *";
	}

	public static string AtRandomMinuteEveryXHours(int hours)
	{
		return $"{new Random().Next(0, 59)} */{hours} * * *";
	}
}
