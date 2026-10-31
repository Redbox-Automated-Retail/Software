using System;

namespace Redbox.Core;

public class CustomSleepEvaluator : ISleepEvaluator
{
	private readonly Func<int, Exception, int> m_function;

	public CustomSleepEvaluator(Func<int, Exception, int> function)
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
