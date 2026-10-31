using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public class CommandBarItemPopupEventArgs : EventArgs
{
	public bool Cancel { get; set; }

	public Control Control { get; private set; }

	public bool Keyboard { get; private set; }

	public Point Position { get; private set; }

	public CommandBarItemPopupMode Mode { get; private set; }

	public CommandBarItemPopupEventArgs(Control control, bool keyboard, Point position, CommandBarItemPopupMode mode)
	{
		Control = control;
		Keyboard = keyboard;
		Position = position;
		Mode = mode;
	}
}
