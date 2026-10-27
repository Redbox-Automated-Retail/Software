namespace Redbox.HAL.Component.Model;

public interface IBarcodeReader
{
	bool IsLicensed { get; }

	IScanResult Scan(string file);
}
