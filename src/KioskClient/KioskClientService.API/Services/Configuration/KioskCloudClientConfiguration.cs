using System;
using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Configuration;

public class KioskCloudClientConfiguration
{
	private static string _timeZoneOffset;

	internal const string ACTIVITY_ID_HEADER_KEY = "x-redbox-activityid";

	internal const string KIOSK_ID_HEADER_KEY = "x-redbox-kioskid";

	internal const string KIOSK_TIMEZONE_OFFSET_HEADER_KEY = "x-redbox-kiosk-timezone-offset";

	public string AuthHeader { get; set; }

	public string APIKey { get; set; }

	public List<Header> GetKioskCloudHeaders(Guid? activityId = null, long? kioskId = null)
	{
		List<Header> list = new List<Header>();
		list.Add(new Header("x-redbox-activityid", (!activityId.HasValue) ? Guid.NewGuid().ToString() : activityId.ToString()));
		list.Add(new Header("x-redbox-kiosk-timezone-offset", GetKioskTimeZoneOffset()));
		if (kioskId.HasValue)
		{
			list.Add(new Header("x-redbox-kioskid", kioskId.ToString()));
		}
		if (!string.IsNullOrWhiteSpace(AuthHeader) && !string.IsNullOrWhiteSpace(APIKey))
		{
			list.Add(new Header(AuthHeader, APIKey));
		}
		return list;
	}

	private static string GetKioskTimeZoneOffset()
	{
		if (_timeZoneOffset == null)
		{
			TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
			_timeZoneOffset = utcOffset.TotalHours.ToString("00") + ":" + utcOffset.Minutes.ToString("00");
		}
		return _timeZoneOffset;
	}
}
