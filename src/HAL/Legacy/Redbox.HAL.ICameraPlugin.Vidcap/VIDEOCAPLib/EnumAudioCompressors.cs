using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("64EE292D-92CA-420E-84DD-DD1EB782D58F")]
[CoClass(typeof(EnumAudioCompressorsClass))]
[ComVisible(true)]
public interface EnumAudioCompressors : IEnumAudioCompressors
{
}
