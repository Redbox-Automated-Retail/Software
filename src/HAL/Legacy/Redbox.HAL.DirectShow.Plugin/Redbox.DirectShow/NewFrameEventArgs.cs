using System;
using System.Drawing;

namespace Redbox.DirectShow;

public class NewFrameEventArgs : EventArgs
{
	private Bitmap frame;

	public Bitmap Frame => frame;

	public NewFrameEventArgs(Bitmap frame)
	{
		this.frame = frame;
	}
}
