using System.Runtime.InteropServices;

namespace RBA_SDK_ComponentModel;

public struct SETTINGS_BLUETOOTH
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
	public string DeviceName;
}
