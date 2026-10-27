using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum FBarcodeType
{
	cibfInterleaved2of5 = 1,
	cibfCode39 = 2,
	cibfCode128 = 4,
	cibfCodabar = 8,
	cibfUpca = 0x10,
	cibfEan8 = 0x20,
	cibfCode93 = 0x40,
	cibfUpce = 0x80,
	cibfEan13 = 0x100,
	cibfAddon2 = 0x200,
	cibfAddon5 = 0x400,
	cibfIndustrial2of5 = 0x10000,
	cibfDatalogic2of5 = 0x20000,
	cibfMatrix2of5 = 0x40000,
	cibfAirline2of5 = 0x80000,
	cibfUcc128 = 0x100000,
	cibfPatch = 0x200000,
	cibfCode32 = 0x800000,
	cibfPostnet = 0x1000000
}
