namespace Redbox.Rental.Model;

public enum ReturnType
{
	Unknown = 1,
	ServiceMenu,
	InventorySync,
	Qlm,
	ExceptionReturn,
	KioskReturn,
	QuickReturn,
	BackwardsDiscReturn,
	UnplayableReturn,
	CrossBorderReturn,
	FailedIRSecurity,
	FailedLabelAudit
}
