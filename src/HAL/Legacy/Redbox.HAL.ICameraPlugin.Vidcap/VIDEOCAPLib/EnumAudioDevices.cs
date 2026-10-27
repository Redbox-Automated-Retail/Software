using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(EnumAudioDevicesClass))]
[Guid("D0CBD8C2-CD57-4124-8774-B940FDA45F54")]
[ComVisible(true)]
public interface EnumAudioDevices : IEnumAudioDevices
{
}
