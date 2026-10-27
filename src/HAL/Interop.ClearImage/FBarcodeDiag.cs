using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum FBarcodeDiag
{
	cibRawData = 0x40,
	cibNoValidation = 0x200,
	cibSymbologyIdentifier = 0x2000
}
