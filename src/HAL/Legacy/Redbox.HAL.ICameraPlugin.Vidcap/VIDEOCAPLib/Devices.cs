using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(DevicesClass))]
[Guid("B0FB0862-4A4F-4395-BB42-8D31903ED98C")]
[ComVisible(true)]
public interface Devices : IDevices
{
}
