using System;

namespace Redbox.HAL.Script.Framework;

internal sealed class QuickReturnCounters
{
	private readonly string[] Counters;

	internal void DoForeach(Action<string> action)
	{
		string[] counters = Counters;
		foreach (string obj in counters)
		{
			action(obj);
		}
	}

	internal QuickReturnCounters()
	{
		Counters = new string[3] { "QuickReturnAttention", "QuickReturnNoInsertDisk", "QuickReturnSuccessful" };
	}
}
