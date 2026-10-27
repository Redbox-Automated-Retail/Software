using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[CoClass(typeof(CiDataMatrixClass))]
[Guid("8B79E556-FAD7-4339-8A8F-2C35D5C42C6F")]
[ComVisible(true)]
public interface CiDataMatrix : ICiDataMatrix
{
}
