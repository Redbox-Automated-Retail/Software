using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[ClassInterface(0)]
[TypeLibType(2)]
[Guid("30AF2DF7-A661-4043-A90E-06C00A638D0F")]
[ComVisible(true)]
public class EnumTVChannelsClass : IEnumTVChannels, EnumTVChannels
{
}
