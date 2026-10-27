using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(PanClass))]
[Guid("C4C0A047-467D-4685-AC3F-D4BE52C9D2FA")]
[ComVisible(true)]
public interface Pan : IPan
{
}
