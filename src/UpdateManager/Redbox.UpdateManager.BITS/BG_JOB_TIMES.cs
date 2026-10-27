using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BG_JOB_TIMES
{
	internal FILETIME CreationTime;

	internal FILETIME ModificationTime;

	internal FILETIME TransferCompletionTime;
}
