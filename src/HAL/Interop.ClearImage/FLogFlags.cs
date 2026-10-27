using System.Runtime.InteropServices;

namespace ClearImage;

[TypeLibType(16)]
[ComVisible(true)]
public enum FLogFlags
{
	ciLogEnable = 1,
	ciLogFileName = 2,
	ciLogSignature = 4,
	ciLogTime = 8,
	ciLogTimeStamp = 0x10
}
