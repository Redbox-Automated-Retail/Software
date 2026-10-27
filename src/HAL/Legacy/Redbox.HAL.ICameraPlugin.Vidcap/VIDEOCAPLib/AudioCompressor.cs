using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("3ECBF508-9BC4-4D74-B384-D2A80453385C")]
[CoClass(typeof(AudioCompressorClass))]
[ComVisible(true)]
public interface AudioCompressor : IAudioCompressor
{
}
