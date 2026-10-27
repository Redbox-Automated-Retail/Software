using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComEventInterface(typeof(_DVideoCapEvents), typeof(_DVideoCapEvents_EventProvider))]
[TypeLibType(16)]
[ComVisible(false)]
public interface _DVideoCapEvents_Event
{
	event _DVideoCapEvents_ClickEventHandler Click;

	event _DVideoCapEvents_MouseMoveEventHandler MouseMove;

	event _DVideoCapEvents_DblClickEventHandler DblClick;

	event _DVideoCapEvents_CapturingEventHandler Capturing;
}
