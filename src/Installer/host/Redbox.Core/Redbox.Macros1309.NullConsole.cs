using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002ENullConsole : Redbox_002EMacros1309_002EIConsole, IDisposable
{
	public bool IsAttachedToConsole => false;

	public void Write(string value)
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
