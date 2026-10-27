namespace DeviceService.ComponentModel;

public interface IApplicationSettings
{
	bool UseSimulator { get; set; }

	string DeviceServiceUrl { get; set; }

	string DeviceServiceClientPath { get; set; }

	bool ActivationFailureRetry { get; set; }

	string DataFilePath { get; set; }
}
