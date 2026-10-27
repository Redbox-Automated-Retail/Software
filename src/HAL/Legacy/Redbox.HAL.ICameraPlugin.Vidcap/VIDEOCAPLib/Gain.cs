using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(GainClass))]
[Guid("06582E8A-0EFD-46CC-9885-7680A438B778")]
[ComVisible(true)]
public interface Gain : IGain
{
}
