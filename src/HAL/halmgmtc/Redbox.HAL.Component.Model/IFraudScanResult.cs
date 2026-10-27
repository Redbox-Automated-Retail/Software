namespace Redbox.HAL.Component.Model;

public interface IFraudScanResult
{
	bool Valid { get; }

	ErrorCodes ScanError { get; }
}
