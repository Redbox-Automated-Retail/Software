using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(EnumAudioInputPinsClass))]
[Guid("ADA661D5-953A-4B0B-A489-E8FE1A12FFB4")]
[ComVisible(true)]
public interface EnumAudioInputPins : IEnumAudioInputPins
{
}
