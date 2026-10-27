using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("BCE12DAC-09D5-4E64-AC70-9C7994B5832C")]
[ClassInterface(0)]
[ComVisible(true)]
public class CiViewClass : ICiView, CiView
{
}
