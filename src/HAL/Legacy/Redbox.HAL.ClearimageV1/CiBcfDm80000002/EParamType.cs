using System.Runtime.InteropServices;

namespace CiBcfDm80000002;

[Guid("E744B3CB-73AC-4152-9AE7-2ED7811C409B")]
[ComVisible(true)]
public enum EParamType
{
	eptNone,
	eptSet,
	eptGet,
	eptSetGet,
	eptRslt,
	eptForm
}
