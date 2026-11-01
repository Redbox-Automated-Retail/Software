using System;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002ETimeSpanExtensions
{
	public static void SpinSleep(this TimeSpan timeSpan)
	{
		using Redbox_002EMacros1309_002EExecutionTimer timer = new Redbox_002EMacros1309_002EExecutionTimer();
		while ((double)timer.ElapsedMilliseconds < timeSpan.TotalMilliseconds)
		{
		}
	}
}
