using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("7E982E10-F1C0-4107-998B-59519710376E")]
[CoClass(typeof(ZoomClass))]
[ComVisible(true)]
public interface Zoom : IZoom
{
}
