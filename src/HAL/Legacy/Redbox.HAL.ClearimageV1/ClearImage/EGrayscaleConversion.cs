using System.Runtime.InteropServices;

namespace ClearImage;

[TypeLibType(16)]
[ComVisible(true)]
public enum EGrayscaleConversion
{
	[TypeLibVar(1088)]
	ciGscStandard = 2,
	[TypeLibVar(1088)]
	ciGscEqual
}
