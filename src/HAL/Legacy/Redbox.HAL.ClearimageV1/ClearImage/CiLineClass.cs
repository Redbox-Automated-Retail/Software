using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("A7A8BF7F-8CB8-49DC-8A76-D0A1A145189C")]
[ClassInterface(0)]
[ComVisible(true)]
public class CiLineClass : ICiLine, CiLine
{
	[DispId(35)]
	public virtual extern ELineDirection Direction
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(35)]
		get;
	}

	[DispId(36)]
	public virtual extern double Thickness
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(36)]
		get;
	}

	[DispId(37)]
	public virtual extern double Angle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(37)]
		get;
	}

	[DispId(38)]
	public virtual extern CiRect Rect
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(38)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(39)]
	public virtual extern CiPoint Start
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(39)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(40)]
	public virtual extern CiPoint End
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(40)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
