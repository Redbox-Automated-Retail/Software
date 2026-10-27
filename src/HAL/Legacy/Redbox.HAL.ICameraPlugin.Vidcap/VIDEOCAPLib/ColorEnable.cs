using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("52C14507-2525-490C-8A53-E07EFE3183B0")]
[CoClass(typeof(ColorEnableClass))]
[ComVisible(true)]
public interface ColorEnable : IColorEnable
{
}
