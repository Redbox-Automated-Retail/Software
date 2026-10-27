using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[TypeLibType(2)]
[ClassInterface(0)]
[Guid("172FADAB-E689-42B5-B435-AB758D1C7DB6")]
[ComVisible(true)]
public class EnumVideoFormatsClass : IEnumVideoFormats, EnumVideoFormats
{
}
