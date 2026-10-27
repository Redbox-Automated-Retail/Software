using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("F2BCF189-0B27-11D4-B5F5-9CC767000000")]
[CoClass(typeof(CiImageClass))]
[ComVisible(true)]
public interface CiImage : ICiImage, _ICiImageEvents_Event
{
}
