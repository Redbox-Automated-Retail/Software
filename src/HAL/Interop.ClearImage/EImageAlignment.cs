using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EImageAlignment
{
	epiaCenter = 0,
	epiaTopLeft = 1,
	epiaTopRight = 2,
	epiaBottomLeft = 3,
	epiaBottomRight = 4,
	epiaTopCenter = 5,
	epiaBottomCenter = 6,
	epiaLeftCenter = 7,
	epiaRightCenter = 8,
	epiaBestFit = 20,
	[TypeLibVar(1216)]
	epiaStretch = 21
}
