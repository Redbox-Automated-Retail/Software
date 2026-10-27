using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("1BB82659-90B0-4BA5-BF26-3D5CCDDF985D")]
[CoClass(typeof(BrightnessClass))]
[ComVisible(true)]
public interface Brightness : IBrightness
{
}
