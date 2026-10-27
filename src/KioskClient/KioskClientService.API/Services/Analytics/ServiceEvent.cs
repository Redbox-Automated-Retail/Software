using System;
using KioskClientService.API.Services.Analytics.KioskDataService;

namespace KioskClientService.API.Services.Analytics;

public class ServiceEvent
{
	public KioskClientEvent EventType { get; set; }

	public DateTime? EventTime { get; set; } = DateTime.Now;
}
