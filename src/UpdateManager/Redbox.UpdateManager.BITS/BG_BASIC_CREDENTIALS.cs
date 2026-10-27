using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct BG_BASIC_CREDENTIALS
{
	[MarshalAs(UnmanagedType.LPWStr)]
	internal string UserName;

	[MarshalAs(UnmanagedType.LPWStr)]
	internal string Password;
}
