using System;

namespace Redbox.Core;

public static class ThrowIf
{
	public static IRethrowEvaluator Always => new CustomRethrowEvaluator((int i, Exception e) => true);

	public static IRethrowEvaluator Custom(Func<int, Exception, bool> function)
	{
		return new CustomRethrowEvaluator(function);
	}

	public static IRethrowEvaluator RetryCountIs(int retryCount)
	{
		return new CustomRethrowEvaluator((int i, Exception e) => i == retryCount);
	}
}
