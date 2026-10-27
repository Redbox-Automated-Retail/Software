using System;

namespace Redbox.Core;

public static class TimeSpanExtensions
{
	public static void SpinSleep(this TimeSpan timeSpan)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		while ((double)executionTimer.ElapsedMilliseconds < timeSpan.TotalMilliseconds)
		{
		}
	}
}
