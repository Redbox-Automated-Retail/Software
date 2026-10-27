using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("42690C73-4494-4755-A5DF-DE9809B2AAE1")]
[CoClass(typeof(EnumTVChannelsClass))]
[ComVisible(true)]
public interface EnumTVChannels : IEnumTVChannels
{
}
