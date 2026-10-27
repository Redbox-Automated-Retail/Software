using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("21DA65F1-9E63-45E3-B081-F78096F9D6C3")]
[CoClass(typeof(CiBarcodeBasicClass))]
[ComVisible(true)]
public interface CiBarcodeBasic : ICiBarcodeBasic
{
}
