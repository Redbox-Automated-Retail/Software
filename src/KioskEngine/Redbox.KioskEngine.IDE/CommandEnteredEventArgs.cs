using System;

namespace Redbox.KioskEngine.IDE;

public class CommandEnteredEventArgs : EventArgs
{
	public string Command { get; private set; }

	public CommandEnteredEventArgs(string command)
	{
		Command = command;
	}
}
