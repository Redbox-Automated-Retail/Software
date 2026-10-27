using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("23A87658-4229-4F90-8E1A-05EBB407B71B")]
[CoClass(typeof(VideoCompressorClass))]
[ComVisible(true)]
public interface VideoCompressor : IVideoCompressor
{
}
