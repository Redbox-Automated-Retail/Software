using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(TVChannelsClass))]
[Guid("5C67BD7A-E0FA-41EF-8A03-F04DE42B9705")]
[ComVisible(true)]
public interface TVChannels : ITVChannels
{
}
