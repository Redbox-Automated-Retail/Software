using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
[ComVisible(false)]
public struct FilterInfo
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Name;

	public IFilterGraph FilterGraph;
}
