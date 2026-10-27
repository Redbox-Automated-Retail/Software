using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("16BE651B-7453-4112-A2D7-F2A6D5F50F0A")]
[CoClass(typeof(ExposureClass))]
[ComVisible(true)]
public interface Exposure : IExposure
{
}
