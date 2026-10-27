using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ClientStoreInfo
{
	public string TimeZoneInfoString { get; set; }

	public DateTime UTCDateTime { get; set; }

	public string Version { get; set; }

	public Dictionary<string, string> Info { get; set; }
}
