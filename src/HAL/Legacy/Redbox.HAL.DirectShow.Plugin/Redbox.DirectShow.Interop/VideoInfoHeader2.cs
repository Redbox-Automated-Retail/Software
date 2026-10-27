using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComVisible(false)]
internal struct VideoInfoHeader2
{
	public RECT SrcRect;

	public RECT TargetRect;

	public int BitRate;

	public int BitErrorRate;

	public long AverageTimePerFrame;

	public int InterlaceFlags;

	public int CopyProtectFlags;

	public int PictAspectRatioX;

	public int PictAspectRatioY;

	public int Reserved1;

	public int Reserved2;

	public BitmapInfoHeader BmiHeader;
}
