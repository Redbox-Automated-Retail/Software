using System.Runtime.InteropServices;

namespace ClearImage;

[TypeLibType(16)]
[ComVisible(true)]
public enum EBiTonalConversion
{
	[TypeLibVar(1088)]
	ciBtcUnknown = 0,
	[TypeLibVar(1088)]
	ciBtcNone = 1,
	[TypeLibVar(1088)]
	ciBtcStandard = 2,
	[TypeLibVar(1088)]
	ciBtcBwOnly = 3,
	[TypeLibVar(1088)]
	ciBtcEdgeOne = 4,
	[TypeLibVar(1088)]
	ciBtcLocalThr = 5,
	[TypeLibVar(1088)]
	ciBtcEdge1H = 220,
	[TypeLibVar(1088)]
	ciBtcEdge1V = 221,
	[TypeLibVar(1088)]
	ciBtcEdge3 = 222,
	[TypeLibVar(1088)]
	ciBtcEdge4 = 223
}
