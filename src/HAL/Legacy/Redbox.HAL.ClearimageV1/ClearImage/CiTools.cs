using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("316BC128-8995-471D-985D-B3E68E87C084")]
[CoClass(typeof(CiToolsClass))]
[ComVisible(true)]
public interface CiTools : ICiTools
{
}
