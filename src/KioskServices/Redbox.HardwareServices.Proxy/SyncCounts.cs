using System.Collections.Generic;

namespace Redbox.HardwareServices.Proxy;

public class SyncCounts
{
	public Dictionary<int, SyncCount> Counts { get; set; } = null!; // set by the object initializer in SyncUnknownProcessor or by the JSON reader
}
