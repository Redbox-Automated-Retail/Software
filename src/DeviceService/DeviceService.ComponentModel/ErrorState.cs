namespace DeviceService.ComponentModel;

public enum ErrorState
{
	None,
	FailedToInitializeConnection,
	FailedToUpdateDevice,
	NoDeviceFound,
	ConnectionError,
	EncryptionError,
	Tampered,
	Unknown
}
