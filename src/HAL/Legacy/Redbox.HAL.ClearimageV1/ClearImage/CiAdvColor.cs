using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[CoClass(typeof(CiAdvColorClass))]
[Guid("8CBBAECA-9716-40CA-B8F6-0E9FF213522A")]
[ComVisible(true)]
public interface CiAdvColor : ICiAdvColor
{
}
