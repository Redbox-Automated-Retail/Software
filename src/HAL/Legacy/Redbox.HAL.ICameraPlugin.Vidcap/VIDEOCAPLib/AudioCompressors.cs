using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("84AACB3E-E9E5-4DD2-A677-8A099A5BFF25")]
[CoClass(typeof(AudioCompressorsClass))]
[ComVisible(true)]
public interface AudioCompressors : IAudioCompressors
{
}
