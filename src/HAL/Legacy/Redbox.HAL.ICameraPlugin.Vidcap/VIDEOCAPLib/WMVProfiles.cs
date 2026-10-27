using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(WMVProfilesClass))]
[Guid("5B0BD413-EBC9-4B48-9F99-414B25C867CE")]
[ComVisible(true)]
public interface WMVProfiles : IWMVProfiles
{
}
