using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("8C531E23-84B0-431E-B39E-849AB24613AF")]
[TypeLibType(4160)]
[ComVisible(true)]
public interface ICiPoint
{
	[DispId(1)]
	int x
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[param: In]
		set;
	}

	[DispId(2)]
	int y
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		[param: In]
		set;
	}
}
