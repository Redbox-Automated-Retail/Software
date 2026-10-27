using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("C1D37E8B-25B6-4FD3-B11B-9A3AA964137C")]
[CoClass(typeof(WMVProfileClass))]
[ComVisible(true)]
public interface WMVProfile : IWMVProfile
{
}
