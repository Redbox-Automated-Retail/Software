using System;

namespace Redbox.UpdateService.Model;

public class StoreInfoChangeSetData
{
	public string TimeZoneInfoString { get; set; }

	public bool CorrectTimeZone { get; set; }

	public DateTime CurrentUTCDateTime { get; set; }

	public bool CorrectDateTime { get; set; }
}
