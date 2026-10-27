using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("020E3D58-1143-4B24-9418-DA245FDF4530")]
[CoClass(typeof(BacklightCompensationClass))]
[ComVisible(true)]
public interface BacklightCompensation : IBacklightCompensation
{
}
