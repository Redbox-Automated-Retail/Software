using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum EBarcodeType
{
	cibInterleaved2of5 = 1,
	cibCode39 = 2,
	cibCode128 = 3,
	cibCodabar = 4,
	cibUpca = 5,
	cibEan8 = 6,
	cibCode93 = 7,
	cibUpce = 8,
	cibEan13 = 9,
	cibAddon2 = 10,
	cibAddon5 = 11,
	cibCode32 = 12,
	cibIndustrial2of5 = 13,
	cibDatalogic2of5 = 14,
	cibMatrix2of5 = 15,
	cibAirline2of5 = 16,
	cibUcc128 = 17,
	cibPatch = 18,
	cibPostnet = 19,
	cibPlanet = 20,
	cibDataMatrix = 30,
	cibPdf417 = 31,
	cib4State = 32,
	cibUspsIntelligentMail = 33,
	cibBpoPostcode = 34,
	cibAustralianPost = 35,
	cibQR = 36
}
