using System;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002ESleepFor
{
	public static Redbox_002EMacros1309_002EISleepEvaluator RandomExponential(int minimumDelay)
	{
		return new Redbox_002EMacros1309_002ERandomExponentialSleepEvaluator(minimumDelay);
	}

	public static Redbox_002EMacros1309_002EISleepEvaluator Milliseconds(int milliseconds)
	{
		return new Redbox_002EMacros1309_002ECustomSleepEvaluator((int retryCount, Exception exception) => milliseconds);
	}

	public static Redbox_002EMacros1309_002EISleepEvaluator Custom(Func<int, Exception, int> function)
	{
		return new Redbox_002EMacros1309_002ECustomSleepEvaluator(function);
	}
}
