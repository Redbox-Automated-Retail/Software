using System.Runtime.InteropServices;

namespace RBA_SDK_ComponentModel;

public struct SETTINGS_ETHERNET
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
	public string IP;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
	public string Port;

	public int SocketDescriptor;
}
