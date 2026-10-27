using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct BG_FILE_RANGE
{
	internal const ulong BG_LENGTH_TO_EOF = ulong.MaxValue;

	internal ulong InitialOffset;

	internal ulong Length;
}
