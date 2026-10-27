using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("C170A41C-6C20-4DB5-BACF-EAD3D4BDD0A6")]
[CoClass(typeof(ContrastClass))]
[ComVisible(true)]
public interface Contrast : IContrast
{
}
