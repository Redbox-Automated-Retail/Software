using System.Collections.Generic;

namespace Redbox.HAL.Management.Console;

internal class CommandHistory
{
	private int currentPosn;

	private readonly List<string> m_commandHistory = new List<string>();

	internal string LastCommand { get; private set; }

	internal void Clear()
	{
		m_commandHistory.Clear();
	}

	internal void Add(string command)
	{
		if (!(command == LastCommand))
		{
			m_commandHistory.Add(command);
			LastCommand = command;
			currentPosn = m_commandHistory.Count;
		}
	}

	internal bool DoesPreviousCommandExist()
	{
		return currentPosn > 0;
	}

	internal bool DoesNextCommandExist()
	{
		return currentPosn < m_commandHistory.Count - 1;
	}

	internal string GetPreviousCommand()
	{
		return m_commandHistory[--currentPosn];
	}

	internal string GetNextCommand()
	{
		return m_commandHistory[++currentPosn];
	}

	internal string[] GetCommandHistory()
	{
		return m_commandHistory.ToArray();
	}
}
