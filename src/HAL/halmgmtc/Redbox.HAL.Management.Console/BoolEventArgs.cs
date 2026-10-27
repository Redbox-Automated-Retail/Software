using System;

namespace Redbox.HAL.Management.Console;

public class BoolEventArgs : EventArgs
{
	public bool State { get; set; }
}
