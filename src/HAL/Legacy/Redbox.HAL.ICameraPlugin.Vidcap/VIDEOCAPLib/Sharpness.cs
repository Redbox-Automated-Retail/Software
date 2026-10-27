using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(SharpnessClass))]
[Guid("CBBB6E85-3FE6-4B3B-92EB-06BFC9BAFC2E")]
[ComVisible(true)]
public interface Sharpness : ISharpness
{
}
