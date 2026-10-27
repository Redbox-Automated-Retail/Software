using System.Runtime.InteropServices;

namespace RBA_SDK_ComponentModel;

public struct SETTINGS_RS232
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
	public string ComPort;

	public uint AutoDetect;

	public uint BaudRate;

	public uint DataBits;

	public uint Parity;

	public uint StopBits;

	public uint FlowControl;
}
