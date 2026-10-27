using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("8B79E556-FAD7-4339-8A8F-2C35D5C42C6F")]
[CoClass(typeof(CiDataMatrixClass))]
[ComVisible(true)]
public interface CiDataMatrix : ICiDataMatrix
{
}
