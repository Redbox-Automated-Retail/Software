using System;

namespace Redbox.KioskEngine.ComponentModel;

public class ViewStateChangedArgs : EventArgs
{
	public ViewStateChangedType Type { get; set; }

	public string FromView { get; set; }

	public string ToView { get; set; }
}
