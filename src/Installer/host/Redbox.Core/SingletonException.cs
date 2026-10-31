using System;

namespace Redbox.Core;

public class SingletonException : ApplicationException
{
	public SingletonException(string message)
		: base(message)
	{
	}
}
