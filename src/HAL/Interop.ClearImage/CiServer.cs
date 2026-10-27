using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[CoClass(typeof(CiServerClass))]
[Guid("A34FC1A7-C73F-4706-886E-C4A33E37C6E5")]
[ComVisible(true)]
public interface CiServer : ICiServer
{
}
