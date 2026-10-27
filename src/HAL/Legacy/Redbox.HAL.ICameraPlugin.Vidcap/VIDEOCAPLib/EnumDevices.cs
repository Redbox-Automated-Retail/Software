using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(EnumDevicesClass))]
[Guid("67C95752-0B29-448B-9F99-5DD468615D7C")]
[ComVisible(true)]
public interface EnumDevices : IEnumDevices
{
}
