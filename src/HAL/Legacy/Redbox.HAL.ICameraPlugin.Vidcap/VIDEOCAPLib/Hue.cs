using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("10615B1E-0619-44AD-AAFD-76D81BDCE929")]
[CoClass(typeof(HueClass))]
[ComVisible(true)]
public interface Hue : IHue
{
}
