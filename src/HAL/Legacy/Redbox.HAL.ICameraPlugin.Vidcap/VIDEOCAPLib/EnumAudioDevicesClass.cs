using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[ClassInterface(0)]
[TypeLibType(2)]
[Guid("E0622347-FD0A-4991-B2E0-41733E69C240")]
[ComVisible(true)]
public class EnumAudioDevicesClass : IEnumAudioDevices, EnumAudioDevices
{
}
