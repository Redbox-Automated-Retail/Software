using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(EnumVideoCompressorsClass))]
[Guid("6713C9DA-187C-4C39-ADDD-9F6F194F63DA")]
[ComVisible(true)]
public interface EnumVideoCompressors : IEnumVideoCompressors
{
}
