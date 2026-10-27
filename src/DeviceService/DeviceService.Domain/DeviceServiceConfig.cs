namespace DeviceService.Domain;

public class DeviceServiceConfig
{
	public bool EnableIngenicoTraceLogging { get; set; }

	public bool EnableVasCommands { get; set; } = true;

	public int CardReadUserInteractionTimeout { get; set; } = 30000;
}
