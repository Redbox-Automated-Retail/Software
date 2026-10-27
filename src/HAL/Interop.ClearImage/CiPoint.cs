using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("8C531E23-84B0-431E-B39E-849AB24613AF")]
[CoClass(typeof(CiPointClass))]
[ComVisible(true)]
public interface CiPoint : ICiPoint
{
}
