using System.Runtime.InteropServices;

namespace AxVIDEOCAPLib;

[ComVisible(true)]
public class _DVideoCapEvents_MouseMoveEvent
{
	public short button;

	public short shift;

	public int x;

	public int y;

	public _DVideoCapEvents_MouseMoveEvent(short button, short shift, int x, int y)
	{
		this.button = button;
		this.shift = shift;
		this.x = x;
		this.y = y;
	}
}
