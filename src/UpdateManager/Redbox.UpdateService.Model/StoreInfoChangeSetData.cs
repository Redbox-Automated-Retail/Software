using System;

namespace Redbox.UpdateService.Model;

public class StoreInfoChangeSetData
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string TimeZoneInfoString { get; set; } = null!;

	public bool CorrectTimeZone { get; set; }

	public DateTime CurrentUTCDateTime { get; set; }

	public bool CorrectDateTime { get; set; }
}
