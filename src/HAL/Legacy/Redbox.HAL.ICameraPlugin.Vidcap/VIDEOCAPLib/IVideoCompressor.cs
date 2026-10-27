using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[InterfaceType(2)]
[Guid("23A87658-4229-4F90-8E1A-05EBB407B71B")]
[TypeLibType(4096)]
[ComVisible(true)]
public interface IVideoCompressor
{
	[DispId(1)]
	string Name
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		set;
	}

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	bool ShowPropertyPage();
}
