using System;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EThrowIf
{
	public static Redbox_002EMacros1309_002EIRethrowEvaluator Always => new Redbox_002EMacros1309_002ECustomRethrowEvaluator((int i, Exception e) => true);

	public static Redbox_002EMacros1309_002EIRethrowEvaluator Custom(Func<int, Exception, bool> function)
	{
		return new Redbox_002EMacros1309_002ECustomRethrowEvaluator(function);
	}

	public static Redbox_002EMacros1309_002EIRethrowEvaluator RetryCountIs(int retryCount)
	{
		return new Redbox_002EMacros1309_002ECustomRethrowEvaluator((int i, Exception e) => i == retryCount);
	}
}
