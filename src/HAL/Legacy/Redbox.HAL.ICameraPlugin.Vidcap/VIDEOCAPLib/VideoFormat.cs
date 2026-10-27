using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(VideoFormatClass))]
[Guid("6034A5E2-AD25-4E94-A429-363481ACEB4A")]
[ComVisible(true)]
public interface VideoFormat : IVideoFormat
{
}
