using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(EnumVideoFormatsClass))]
[Guid("7DA61D16-5E79-40E4-92D3-E262BB10479F")]
[ComVisible(true)]
public interface EnumVideoFormats : IEnumVideoFormats
{
}
