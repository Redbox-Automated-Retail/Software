using System.Runtime.InteropServices;

namespace CiBcfKo86572612;

[TypeLibType(16)]
[ComEventInterface(typeof(__CiBcFlow), typeof(__CiBcFlow_EventProvider))]
[ComVisible(false)]
public interface __CiBcFlow_Event
{
	event __CiBcFlow_BarcodeFoundEventHandler BarcodeFound;
}
