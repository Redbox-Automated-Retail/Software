using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(AudioInputPinsClass))]
[Guid("84FE9018-73A2-4A83-B2E3-526B34D6907A")]
[ComVisible(true)]
public interface AudioInputPins : IAudioInputPins
{
}
