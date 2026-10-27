using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.Services.Analytics.KioskDataService;

public class KioskClientEventRequest
{
	public long KioskId { get; set; }

	public string AssemblyVersion { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public KioskClientEvent EventType { get; set; }

	public DateTime EventTime { get; set; } = DateTime.Now;

	public DateTime EventTimeUtc => EventTime.ToUniversalTime();

	public KioskClientEventRequest(long kioskId, KioskClientEvent eventType, DateTime? eventTime)
	{
		KioskId = kioskId;
		AssemblyVersion = GetType().Assembly.GetName().Version.ToString();
		EventType = eventType;
		if (eventTime.HasValue)
		{
			EventTime = eventTime.Value;
		}
	}
}
