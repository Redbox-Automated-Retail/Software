using System;

namespace Redbox.HAL.Core;

public class DefaultConsole : IConsole, IDisposable
{
	private readonly bool m_attachedToParentConsole;

	public bool IsAttachedToConsole => m_attachedToParentConsole;

	public DefaultConsole()
	{
		m_attachedToParentConsole = ConsoleTool.AttachConsoleToParentProcess();
		if (m_attachedToParentConsole && Console.CursorTop + 1 < Console.BufferHeight)
		{
			Console.SetCursorPosition(0, Console.CursorTop + 1);
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
			ConsoleTool.FreeAttachedConsole();
		}
	}
}
