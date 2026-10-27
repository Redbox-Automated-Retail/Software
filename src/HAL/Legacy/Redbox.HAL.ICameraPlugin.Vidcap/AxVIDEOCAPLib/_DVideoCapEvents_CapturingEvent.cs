using System.Runtime.InteropServices;

namespace AxVIDEOCAPLib;

[ComVisible(true)]
public class _DVideoCapEvents_CapturingEvent
{
	public int lFrame;

	public int lDropFrame;

	public _DVideoCapEvents_CapturingEvent(int lFrame, int lDropFrame)
	{
		this.lFrame = lFrame;
		this.lDropFrame = lDropFrame;
	}
}
