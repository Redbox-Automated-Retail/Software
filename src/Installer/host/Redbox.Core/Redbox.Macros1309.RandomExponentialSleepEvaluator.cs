using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002ERandomExponentialSleepEvaluator : Redbox_002EMacros1309_002ECustomSleepEvaluator
{
	public Redbox_002EMacros1309_002ERandomExponentialSleepEvaluator()
		: this(0)
	{
	}

	public Redbox_002EMacros1309_002ERandomExponentialSleepEvaluator(int minimumDelay)
		: base((int retryCount, Exception exception) => SleepMilliseconds(retryCount, minimumDelay))
	{
		Redbox_002EMacros1309_002ERandomExponentialSleepEvaluator obj = this;
	}

	private static int SleepMilliseconds(int retryCount, int minimumDelay)
	{
		Random random = new Random(Guid.NewGuid().GetHashCode());
		return random.Next((int)Math.Pow(retryCount, 2.0) + minimumDelay, (int)Math.Pow(retryCount + 1, 2.0) + 1 + minimumDelay);
	}
}
