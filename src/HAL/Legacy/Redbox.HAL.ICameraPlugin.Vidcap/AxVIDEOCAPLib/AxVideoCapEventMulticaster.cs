using System;
using System.Runtime.InteropServices;
using VIDEOCAPLib;

namespace AxVIDEOCAPLib;

[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public class AxVideoCapEventMulticaster : _DVideoCapEvents
{
	private AxVideoCap parent;

	public AxVideoCapEventMulticaster(AxVideoCap parent)
	{
		this.parent = parent;
	}

	public virtual void Click()
	{
		EventArgs e = new EventArgs();
		parent.RaiseOnClickEvent(parent, e);
	}

	public virtual void MouseMove(short button, short shift, int x, int y)
	{
		_DVideoCapEvents_MouseMoveEvent e = new _DVideoCapEvents_MouseMoveEvent(button, shift, x, y);
		parent.RaiseOnMouseMoveEvent(parent, e);
	}

	public virtual void DblClick()
	{
		EventArgs e = new EventArgs();
		parent.RaiseOnDblClick(parent, e);
	}

	public virtual void Capturing(int lFrame, int lDropFrame)
	{
		_DVideoCapEvents_CapturingEvent e = new _DVideoCapEvents_CapturingEvent(lFrame, lDropFrame);
		parent.RaiseOnCapturing(parent, e);
	}
}
