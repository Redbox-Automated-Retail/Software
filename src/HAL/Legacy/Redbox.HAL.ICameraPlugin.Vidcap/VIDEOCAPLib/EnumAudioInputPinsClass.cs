using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[ClassInterface(0)]
[Guid("B0176138-D4E7-49F3-8942-DE9FE6DB66BD")]
[TypeLibType(2)]
[ComVisible(true)]
public class EnumAudioInputPinsClass : IEnumAudioInputPins, EnumAudioInputPins
{
}
