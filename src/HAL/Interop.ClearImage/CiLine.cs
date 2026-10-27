using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("B8E5EE38-BB0D-4561-B8D3-26C18C8817EE")]
[CoClass(typeof(CiLineClass))]
[ComVisible(true)]
public interface CiLine : ICiLine
{
}
