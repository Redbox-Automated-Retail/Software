using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EFileFormat
{
	ciEXT = 1,
	ciBMP = 3,
	ciPCX = 4,
	ciTIFF = 5,
	ciTIFF_G3_1D = 6,
	ciTIFF_G4 = 7,
	ciTIFF_NEOL = 8,
	ciTIFF_JPEG = 20,
	ciGIF = 102,
	ciJPG = 103,
	ciPNG = 104,
	ciICO = 106,
	ciTGA = 108,
	ciWBMP = 110,
	ciJBG = 113,
	ciJP2 = 114,
	ciJPC = 115,
	ciPGX = 116,
	ciPNM = 117,
	ciTIFF_LZW = 21,
	cifPDF = 22,
	cifICL = 23
}
