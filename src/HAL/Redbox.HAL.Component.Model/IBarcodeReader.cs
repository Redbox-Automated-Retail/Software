namespace Redbox.HAL.Component.Model;

public interface IBarcodeReader
{
	bool IsLicensed { get; }

	BarcodeServices Service { get; }

	IScanResult Scan(string file);

	IScanResult Scan(ISnapResult sr);
}
