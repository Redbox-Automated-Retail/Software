namespace DeviceService.ComponentModel;

public enum ResponseStatus
{
	Success,
	Errored,
	Disconnected,
	Innoperable,
	CallAlreadyInProgress,
	Timeout,
	Cancelled,
	Tampered,
	Blocked
}
