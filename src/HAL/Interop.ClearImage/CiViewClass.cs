using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[ClassInterface(0)]
[Guid("BCE12DAC-09D5-4E64-AC70-9C7994B5832C")]
[ComVisible(true)]
public class CiViewClass : ICiView, CiView
{
}
