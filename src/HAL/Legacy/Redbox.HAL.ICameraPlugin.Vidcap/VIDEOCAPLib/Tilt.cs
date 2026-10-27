using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("0B8546C0-B84E-4B00-9463-DC1880661546")]
[CoClass(typeof(TiltClass))]
[ComVisible(true)]
public interface Tilt : ITilt
{
}
