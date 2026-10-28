using System;

namespace Redbox.HAL.Core;

public class SingletonException : ApplicationException
{
	public SingletonException(string message)
		: base(message)
	{
	}
}
