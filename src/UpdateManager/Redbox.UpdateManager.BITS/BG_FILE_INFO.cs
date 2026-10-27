using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BG_FILE_INFO
{
	[MarshalAs(UnmanagedType.LPWStr)]
	internal string RemoteName;

	[MarshalAs(UnmanagedType.LPWStr)]
	internal string LocalName;
}
