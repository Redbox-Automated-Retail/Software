using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(VideoFormatsClass))]
[Guid("DD55C4EB-00E7-46AD-8137-E5A3EFA206E8")]
[ComVisible(true)]
public interface VideoFormats : IVideoFormats
{
}
