using System.Runtime.InteropServices;

namespace ClearImage;

[ComEventInterface(typeof(_ICiImageEvents), typeof(_ICiImageEvents_EventProvider))]
[TypeLibType(16)]
[ComVisible(false)]
public interface _ICiImageEvents_Event
{
	event _ICiImageEvents_LogEventHandler Log;
}
