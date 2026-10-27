namespace Redbox.KioskServices.Proxy.ComponentModel.Loyalty;

public enum VendStatus
{
	Vended = 0,
	HardwareError = 7,
	DiskNotTaken = 8,
	EmptyOrStuck = 11,
	KioskFull = 14,
	NotVended = 28,
	CriticalHardwareError = 9,
	CustomerDisabled = 15
}
