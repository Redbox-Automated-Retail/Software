using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EScaleType
{
	ciScaleSkipLines = 3,
	ciScaleMergeLinePixels = 2,
	ciScaleMergeLineGaps = 1,
	ciScaleThreshold = 101
}
