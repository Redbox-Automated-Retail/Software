using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("095BDAE6-D5FE-44E0-ACEB-F7ED47B60796")]
[CoClass(typeof(IrisClass))]
[ComVisible(true)]
public interface Iris : IIris
{
}
