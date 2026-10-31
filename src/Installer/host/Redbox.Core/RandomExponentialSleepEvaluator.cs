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
		RandomExponentialSleepEvaluator randomExponentialSleepEvaluator = this;
	}

	private static int SleepMilliseconds(int retryCount, int minimumDelay)
	{
		Random random = new Random(Guid.NewGuid().GetHashCode());
		return random.Next((int)Math.Pow(retryCount, 2.0) + minimumDelay, (int)Math.Pow(retryCount + 1, 2.0) + 1 + minimumDelay);
	}
}
