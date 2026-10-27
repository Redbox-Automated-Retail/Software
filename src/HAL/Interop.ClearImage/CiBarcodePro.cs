using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("BDDB0244-0CFD-11D4-B5F8-B89D57000000")]
[CoClass(typeof(CiBarcodeProClass))]
[ComVisible(true)]
public interface CiBarcodePro : ICiBarcodePro
{
}
