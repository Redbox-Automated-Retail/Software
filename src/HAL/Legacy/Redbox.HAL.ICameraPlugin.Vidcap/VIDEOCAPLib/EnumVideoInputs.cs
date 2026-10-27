using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(EnumVideoInputsClass))]
[Guid("FBB69778-9498-4A4E-A4F2-73F56A4BE3F6")]
[ComVisible(true)]
public interface EnumVideoInputs : IEnumVideoInputs
{
}
