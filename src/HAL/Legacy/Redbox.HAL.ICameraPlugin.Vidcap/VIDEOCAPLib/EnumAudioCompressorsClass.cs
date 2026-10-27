using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[TypeLibType(2)]
[ClassInterface(0)]
[Guid("1E4EDCB3-9F94-442A-A531-438E1E562180")]
[ComVisible(true)]
public class EnumAudioCompressorsClass : IEnumAudioCompressors, EnumAudioCompressors
{
}
