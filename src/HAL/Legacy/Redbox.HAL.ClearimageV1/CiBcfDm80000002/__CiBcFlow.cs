using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ClearImage;

namespace CiBcfDm80000002;

[ComImport]
[Guid("2D65EF60-1DCC-4CEC-AE55-F2A4106D2D52")]
[InterfaceType(2)]
[TypeLibType(4240)]
[ComVisible(true)]
public interface __CiBcFlow
{
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1)]
	void BarcodeFound([In][Out][MarshalAs(UnmanagedType.Interface)] ref CiBarcode Bc);
}
