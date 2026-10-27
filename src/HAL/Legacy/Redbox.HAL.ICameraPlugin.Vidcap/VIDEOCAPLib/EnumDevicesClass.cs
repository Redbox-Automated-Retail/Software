using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[TypeLibType(2)]
[ClassInterface(0)]
[Guid("A7FCBEEB-3847-4AB7-96A5-E6CA1600D810")]
[ComVisible(true)]
public class EnumDevicesClass : IEnumDevices, EnumDevices
{
}
