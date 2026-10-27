namespace Redbox.HAL.Component.Model;

public interface IFraudSensor
{
	ErrorCodes LastPostResult { get; }

	ErrorCodes Initialize(IFormattedLog log);

	ErrorCodes Shutdown();

	ErrorCodes Reset(IFormattedLog log);

	ErrorCodes StartScan();

	ErrorCodes StopScan();

	IFraudScanResult Validate();
}
