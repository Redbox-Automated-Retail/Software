using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComVisible(false)]
internal struct VideoInfoHeader
{
	public RECT SrcRect;

	public RECT TargetRect;

	public int BitRate;

	public int BitErrorRate;

	public long AverageTimePerFrame;

	public BitmapInfoHeader BmiHeader;
}
