namespace KioskClientService.API.Services.Transaction;

public enum VendStatus
{
	Vended = 0,
	HardwareError = 7,
	DiskNotTaken = 8,
	CriticalHardwareError = 9,
	EmptyOrStuck = 11,
	KioskFull = 14,
	CustomerDisabled = 15,
	NotVended = 28
}
