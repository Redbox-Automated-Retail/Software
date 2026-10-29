using System;

namespace Redbox.Core;

public class NullConsole : IConsole, IDisposable
{
	public bool IsAttachedToConsole => false;

	public void Write(string value)
	{
	}

	public void Write(string format, params object[] parms)
	{
	}

	public void WriteLine()
	{
	}

	public void WriteLine(object value)
	{
	}

	public void WriteLine(string value)
	{
	}

	public void WriteLine(string format, params object[] parms)
	{
	}

	public void Dispose()
	{
	}
}
