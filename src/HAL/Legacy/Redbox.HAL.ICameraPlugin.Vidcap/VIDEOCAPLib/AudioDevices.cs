using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("D4E1418D-11CD-4AEF-BBBC-5518AB049180")]
[CoClass(typeof(AudioDevicesClass))]
[ComVisible(true)]
public interface AudioDevices : IAudioDevices
{
}
