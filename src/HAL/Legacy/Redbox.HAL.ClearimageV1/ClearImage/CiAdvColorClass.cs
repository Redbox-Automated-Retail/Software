using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[ClassInterface(0)]
[Guid("3022D35D-0127-4C24-B1F0-1C66A831807E")]
[ComVisible(true)]
public class CiAdvColorClass : ICiAdvColor, CiAdvColor
{
	[DispId(1)]
	public virtual extern CiImage Image
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
	public virtual extern void ConvertToBitonal([In] EBiTonalConversion mode = EBiTonalConversion.ciBtcStandard, [In] int Par0 = 0, [In] int par1 = 0, [In] int par2 = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	public virtual extern void ConvertToGrayscale([In] EGrayscaleConversion mode = EGrayscaleConversion.ciGscStandard, [In] int Par0 = 0, [In] int par1 = 0, [In] int par2 = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	[TypeLibFunc(1088)]
	public virtual extern void ScaleToDpi([In] EScaleBmpType mode = EScaleBmpType.ciScaleBestQuality, [In] int Par0 = 0);
}
