using System;

namespace Redbox.Core;

public class CustomRethrowEvaluator : IRethrowEvaluator
{
	private readonly Func<int, Exception, bool> m_function;

	public CustomRethrowEvaluator(Func<int, Exception, bool> function)
	{
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		m_function = function;
	}

	public bool Rethrow(int retryCount, Exception exception)
	{
		return m_function(retryCount, exception);
	}
}
