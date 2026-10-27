using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[ClassInterface(0)]
[Guid("8C10F0D1-96BC-4C8C-BA9C-73F3F5964C61")]
[TypeLibType(2)]
[ComVisible(true)]
public class AudioCompressorClass : IAudioCompressor, AudioCompressor
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
