using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("59A0E32D-5050-47F5-A21B-F00397A21FCC")]
[TypeLibType(4160)]
[ComVisible(true)]
public interface ICiObject
{
	[DispId(3)]
	int Pixels
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		get;
	}

	[DispId(4)]
	int Intervals
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		get;
	}

	[DispId(5)]
	CiRect Rect
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

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

	// C# has no syntax for parameterized property 'cx2l'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(7)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(7)]
	[TypeLibFunc(1088)]
	int get_cx2l([In] int i);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(7)]
	void set_cx2l([In] int i, [In] int pVal);
}
