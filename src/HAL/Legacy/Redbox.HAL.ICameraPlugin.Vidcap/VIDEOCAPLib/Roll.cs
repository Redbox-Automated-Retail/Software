using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("C9D5CEED-68BE-40AB-89F2-0D38CD0B0660")]
[CoClass(typeof(RollClass))]
[ComVisible(true)]
public interface Roll : IRoll
{
}
