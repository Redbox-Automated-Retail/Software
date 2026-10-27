using System.Runtime.InteropServices;

namespace CiBcfKo86572612;

[Guid("0D84034A-2C47-4116-8AFE-6454559B43D4")]
[ComVisible(true)]
public enum EBarcodeFoundProcess
{
	ebfpContinue,
	ebfpIgnore,
	ebfpAbort,
	ebfpIgnoreAndAbort
}
