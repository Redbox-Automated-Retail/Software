using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EBarcodeRotation
{
	cibRotNone = 1,
	cibRotUpsideDown,
	cibRotLeft,
	cibRotRight,
	cibRotDiag
}
