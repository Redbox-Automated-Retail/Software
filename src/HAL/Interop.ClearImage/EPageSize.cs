using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EPageSize
{
	epgsCustom = 4,
	epgsOriginal = 5,
	epgsLetter = 10,
	epgsLegal = 11,
	epgsLedger = 12,
	epgsA4 = 20,
	[TypeLibVar(1216)]
	epgsImage = 40,
	[TypeLibVar(1216)]
	epgsNearest = 41,
	[TypeLibVar(1216)]
	epgsNearestStd = 42,
	[TypeLibVar(1216)]
	epgsNearestMetric = 43
}
