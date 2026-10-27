using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct BG_AUTH_CREDENTIALS_UNION
{
	internal BG_BASIC_CREDENTIALS Basic;
}
