using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(VideoCapClass))]
[Guid("43510FCA-3010-43D1-9952-B8B712B9869C")]
[ComVisible(true)]
public interface VideoCap : _DVideoCap, _DVideoCapEvents_Event
{
}
