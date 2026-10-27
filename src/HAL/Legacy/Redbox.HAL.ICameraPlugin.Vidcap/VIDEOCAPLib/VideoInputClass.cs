using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("5B76CA84-FAAC-4884-9FE0-B885CE3F141F")]
[ClassInterface(0)]
[TypeLibType(2)]
[ComVisible(true)]
public class VideoInputClass : IVideoInput, VideoInput
{
	[DispId(1)]
	public virtual extern string Name
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		set;
	}
}
