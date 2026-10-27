using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[TypeLibType(4160)]
[Guid("8CBBAECA-9716-40CA-B8F6-0E9FF213522A")]
[ComVisible(true)]
public interface ICiAdvColor
{
	[DispId(1)]
	CiImage Image
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	void ConvertToBitonal([In] EBiTonalConversion mode = EBiTonalConversion.ciBtcStandard, [In] int Par0 = 0, [In] int par1 = 0, [In] int par2 = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	void ConvertToGrayscale([In] EGrayscaleConversion mode = EGrayscaleConversion.ciGscStandard, [In] int Par0 = 0, [In] int par1 = 0, [In] int par2 = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	[TypeLibFunc(1088)]
	void ScaleToDpi([In] EScaleBmpType mode = EScaleBmpType.ciScaleBestQuality, [In] int Par0 = 0);
}
