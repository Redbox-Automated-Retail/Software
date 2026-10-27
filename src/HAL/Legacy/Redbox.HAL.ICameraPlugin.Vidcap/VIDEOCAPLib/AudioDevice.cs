using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(AudioDeviceClass))]
[Guid("6520FF57-B865-4069-A1B2-2BD7C538A66C")]
[ComVisible(true)]
public interface AudioDevice : IAudioDevice
{
}
