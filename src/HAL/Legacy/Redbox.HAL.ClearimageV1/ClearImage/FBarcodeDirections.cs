using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum FBarcodeDirections
{
	cibHorz = 1,
	cibVert = 2,
	cibDiag = 4
}
