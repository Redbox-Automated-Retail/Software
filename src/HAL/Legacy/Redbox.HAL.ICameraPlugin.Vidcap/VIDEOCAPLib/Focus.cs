using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("41AF3A64-ACB2-4906-A9C4-07D7BA638A6A")]
[CoClass(typeof(FocusClass))]
[ComVisible(true)]
public interface Focus : IFocus
{
}
