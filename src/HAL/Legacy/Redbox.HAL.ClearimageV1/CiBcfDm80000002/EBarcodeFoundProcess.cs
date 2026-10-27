using System.Runtime.InteropServices;

namespace CiBcfDm80000002;

[Guid("2C242AEE-E927-4F28-882F-26EA4A16276A")]
[ComVisible(true)]
public enum EBarcodeFoundProcess
{
	ebfpContinue,
	ebfpIgnore,
	ebfpAbort,
	ebfpIgnoreAndAbort
}
