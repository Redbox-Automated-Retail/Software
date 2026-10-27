using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[TypeLibType(4160)]
[Guid("B8E5EE38-BB0D-4561-B8D3-26C18C8817EE")]
[ComVisible(true)]
public interface ICiLine
{
	[DispId(35)]
	ELineDirection Direction
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(35)]
		get;
	}

	[DispId(36)]
	double Thickness
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(36)]
		get;
	}

	[DispId(37)]
	double Angle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(37)]
		get;
	}

	[DispId(38)]
	CiRect Rect
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(38)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(39)]
	CiPoint Start
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(39)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(40)]
	CiPoint End
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(40)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
