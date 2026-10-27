using System;

namespace Redbox.Core;

public class TimeoutEntry
{
	public DateTime Timeout { get; set; }

	public ITimeoutSink TimeoutSink { get; set; }
}
