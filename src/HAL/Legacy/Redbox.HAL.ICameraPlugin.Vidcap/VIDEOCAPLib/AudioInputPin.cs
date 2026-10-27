using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(AudioInputPinClass))]
[Guid("A30E3E82-362A-4398-9780-FBB6E9983ED7")]
[ComVisible(true)]
public interface AudioInputPin : IAudioInputPin
{
}
