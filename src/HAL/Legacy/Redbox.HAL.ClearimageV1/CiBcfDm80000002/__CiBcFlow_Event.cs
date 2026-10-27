using System.Runtime.InteropServices;

namespace CiBcfDm80000002;

[ComVisible(false)]
[TypeLibType(16)]
[ComEventInterface(typeof(__CiBcFlow), typeof(__CiBcFlow_EventProvider))]
public interface __CiBcFlow_Event
{
	event __CiBcFlow_BarcodeFoundEventHandler BarcodeFound;
}
