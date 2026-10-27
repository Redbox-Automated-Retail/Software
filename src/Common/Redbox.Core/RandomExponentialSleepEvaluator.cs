using System;

namespace Redbox.Core;

public class RandomExponentialSleepEvaluator : CustomSleepEvaluator
{
	public RandomExponentialSleepEvaluator()
		: this(0)
	{
	}

	public RandomExponentialSleepEvaluator(int minimumDelay)
		: base((int retryCount, Exception exception) => SleepMilliseconds(retryCount, minimumDelay))
	{
	}

	private static int SleepMilliseconds(int retryCount, int minimumDelay)
	{
		return new Random(Guid.NewGuid().GetHashCode()).Next((int)Math.Pow(retryCount, 2.0) + minimumDelay, (int)Math.Pow(retryCount + 1, 2.0) + 1 + minimumDelay);
	}
}
