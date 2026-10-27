using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EBorderExtractFlags
{
	ciBexBorder = 1,
	ciBexBorderDeskew,
	ciBexBorderDeskewCrop,
	ciBexDeskewCrop
}
