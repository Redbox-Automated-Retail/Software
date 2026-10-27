using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[TypeLibType(2)]
[Guid("F7DC9E32-6AE4-476A-9DCC-E12FDCC106FC")]
[ClassInterface(0)]
[ComVisible(true)]
public class DeviceClass : IDevice, Device
{
	[DispId(1)]
	public virtual extern string Name
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		set;
	}
}
