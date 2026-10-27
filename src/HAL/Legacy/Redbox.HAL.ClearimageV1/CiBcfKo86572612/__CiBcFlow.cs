using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ClearImage;

namespace CiBcfKo86572612;

[ComImport]
[TypeLibType(4240)]
[Guid("FA4F8E5F-0481-4765-9DAB-6E01B66D2DED")]
[InterfaceType(2)]
[ComVisible(true)]
public interface __CiBcFlow
{
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1)]
	void BarcodeFound([In][Out][MarshalAs(UnmanagedType.Interface)] ref CiBarcode Bc);
}
