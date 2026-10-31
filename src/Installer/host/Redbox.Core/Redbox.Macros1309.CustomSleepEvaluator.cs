using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002ECustomSleepEvaluator : Redbox_002EMacros1309_002EISleepEvaluator
{
	private readonly Func<int, Exception, int> m_function;

	public Redbox_002EMacros1309_002ECustomSleepEvaluator(Func<int, Exception, int> function)
	{
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		m_function = function;
	}

	public int SleepMilliseconds(int retryCount, Exception exception)
	{
		return m_function(retryCount, exception);
	}
}
