using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("D50FE1C2-962A-439F-85F1-D2A1E87A3576")]
[CoClass(typeof(VideoInputClass))]
[ComVisible(true)]
public interface VideoInput : IVideoInput
{
}
