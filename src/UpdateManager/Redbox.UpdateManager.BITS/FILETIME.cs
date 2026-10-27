using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FILETIME
{
	internal uint dwLowDateTime;

	internal uint dwHighDateTime;
}
