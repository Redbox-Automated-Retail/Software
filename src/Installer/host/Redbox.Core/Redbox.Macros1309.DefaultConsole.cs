using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EDefaultConsole : Redbox_002EMacros1309_002EIConsole, IDisposable
{
	private readonly bool m_attachedToParentConsole;

	public bool IsAttachedToConsole => m_attachedToParentConsole;

	public Redbox_002EMacros1309_002EDefaultConsole()
	{
		m_attachedToParentConsole = Redbox_002EMacros1309_002EConsoleTool.AttachConsoleToParentProcess();
		try
		{
			if (m_attachedToParentConsole && Console.CursorTop + 1 < Console.BufferHeight)
			{
				Console.SetCursorPosition(0, Console.CursorTop + 1);
			}
		}
		catch
		{
		}
	}

	public void Write(string value)
	{
		Console.Write(value);
	}

	public void WriteLine()
	{
		Console.WriteLine();
	}

	public void WriteLine(object value)
	{
		Console.WriteLine(value);
	}

	public void WriteLine(string value)
	{
		Console.WriteLine(value);
	}

	public void WriteLine(string format, params object[] parms)
	{
		Console.WriteLine(format, parms);
	}

	public void Dispose()
	{
		if (m_attachedToParentConsole)
		{
			Redbox_002EMacros1309_002EConsoleTool.FreeAttachedConsole();
		}
	}
}
