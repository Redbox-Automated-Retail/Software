using System;

namespace Redbox.DirectShow;

public class VideoSourceErrorEventArgs : EventArgs
{
	private string description;

	public string Description => description;

	public VideoSourceErrorEventArgs(string description)
	{
		this.description = description;
	}
}
