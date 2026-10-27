using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("4ED88244-0BE1-11D4-B5F6-009FC6000000")]
[CoClass(typeof(CiRectClass))]
[ComVisible(true)]
public interface CiRect : ICiRect
{
}
