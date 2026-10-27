using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("39EDC387-7C09-48CD-8C6A-9FA8F77148B8")]
[CoClass(typeof(DeviceClass))]
[ComVisible(true)]
public interface Device : IDevice
{
}
