namespace Redbox.HAL.Component.Model;

public interface IDiskValidatorService : IConfigurationObserver
{
	bool IsConfigured { get; }

	IFraudSensor FraudSensor { get; }

	IFraudScanResult Validate(string matrix);

	void CorrectErroredPost(IFormattedLog log);
}
