using System;

namespace Redbox.HAL.Management.Console;

public class CommandEnteredEventArgs : EventArgs
{
	public string Command { get; private set; }

	public CommandEnteredEventArgs(string command)
	{
		Command = command;
	}
}
