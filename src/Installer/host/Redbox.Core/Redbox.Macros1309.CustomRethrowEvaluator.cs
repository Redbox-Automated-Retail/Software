using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002ECustomRethrowEvaluator : Redbox_002EMacros1309_002EIRethrowEvaluator
{
	private readonly Func<int, Exception, bool> m_function;

	public Redbox_002EMacros1309_002ECustomRethrowEvaluator(Func<int, Exception, bool> function)
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
