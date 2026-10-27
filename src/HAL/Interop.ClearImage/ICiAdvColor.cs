using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("8CBBAECA-9716-40CA-B8F6-0E9FF213522A")]
[TypeLibType(4160)]
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

	// C# has no syntax for parameterized property 'cx2l'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(5)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	[TypeLibFunc(1088)]
	int get_cx2l([In] int i);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(5)]
	void set_cx2l([In] int i, [In] int pVal);

	// C# has no syntax for parameterized property 'cx3d'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(6)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	[TypeLibFunc(1088)]
	double get_cx3d([In] int i);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(6)]
	void set_cx3d([In] int i, [In] double pVal);
}
