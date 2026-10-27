using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(false)]
[ComEventInterface(typeof(_ICiImageEvents), typeof(_ICiImageEvents_EventProvider))]
[TypeLibType(16)]
public interface _ICiImageEvents_Event
{
	event _ICiImageEvents_LogEventHandler Log;
}
