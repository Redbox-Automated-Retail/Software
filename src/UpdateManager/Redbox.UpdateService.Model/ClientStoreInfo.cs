using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ClientStoreInfo
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string TimeZoneInfoString { get; set; } = null!;

	public DateTime UTCDateTime { get; set; }

	public string Version { get; set; } = null!;

	public Dictionary<string, string> Info { get; set; } = null!;
}
