using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct BG_JOB_REPLY_PROGRESS
{
	internal ulong BytesTotal;

	internal ulong BytesTransferred;
}
