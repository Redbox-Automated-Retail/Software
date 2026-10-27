using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("4540AB55-1050-4DEE-8462-8FA0EEC7ECF7")]
[CoClass(typeof(EnumWMVProfilesClass))]
[ComVisible(true)]
public interface EnumWMVProfiles : IEnumWMVProfiles
{
}
