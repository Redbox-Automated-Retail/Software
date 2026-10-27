using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[ClassInterface(0)]
[Guid("A494DEC1-F2DB-4C18-A62E-3BAB80488ECC")]
[TypeLibType(2)]
[ComVisible(true)]
public class EnumVideoCompressorsClass : IEnumVideoCompressors, EnumVideoCompressors
{
}
