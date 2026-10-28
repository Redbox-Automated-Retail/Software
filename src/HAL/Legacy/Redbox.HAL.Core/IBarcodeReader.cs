namespace Redbox.HAL.Core;

public interface IBarcodeReader
{
	IScanResult Scan(string file, int expectedCodes);
}
