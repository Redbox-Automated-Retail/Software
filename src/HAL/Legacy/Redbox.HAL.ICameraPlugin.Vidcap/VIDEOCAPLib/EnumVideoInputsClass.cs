using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[TypeLibType(2)]
[ClassInterface(0)]
[Guid("5C0F2AB1-83B4-4F40-A889-8EF3CEAB34BF")]
[ComVisible(true)]
public class EnumVideoInputsClass : IEnumVideoInputs, EnumVideoInputs
{
}
