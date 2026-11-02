using DeviceService.ComponentModel;

namespace DeviceService.WebApi;

public class ApplicationSettings : IApplicationSettings
{
	public bool UseSimulator { get; set; }

	// The three paths below are bound from the host's configuration at start-up.
	public string DataFilePath { get; set; } = null!;

	public string DeviceServiceUrl { get; set; } = null!;

	public string DeviceServiceClientPath { get; set; } = null!;

	public bool ActivationFailureRetry { get; set; }
}
