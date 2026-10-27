using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("19534C76-9593-4369-B2EF-7163D0AC5553")]
[CoClass(typeof(VideoCompressorsClass))]
[ComVisible(true)]
public interface VideoCompressors : IVideoCompressors
{
}
