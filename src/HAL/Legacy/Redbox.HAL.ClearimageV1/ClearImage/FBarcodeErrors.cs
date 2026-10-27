using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum FBarcodeErrors
{
	cibDecoding = 1,
	cibBadChecksum = 2,
	cibBadStartChar = 4,
	cibBadStopChar = 8,
	cibBadData = 0x10
}
