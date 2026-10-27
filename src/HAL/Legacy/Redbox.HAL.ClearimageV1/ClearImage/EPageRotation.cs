using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EPageRotation
{
	ciRotUnknown = 0,
	ciRotNone = 1,
	ciRotUpsideDown = 2,
	ciRotPortrait = 3,
	ciRotLeft = 4,
	ciRotRight = 8,
	ciRotLeftOrRight = 12
}
