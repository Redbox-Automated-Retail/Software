using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("98AFD5D0-A728-498D-B1DE-0D811D719793")]
[CoClass(typeof(CiViewClass))]
[ComVisible(true)]
public interface CiView : ICiView
{
}
