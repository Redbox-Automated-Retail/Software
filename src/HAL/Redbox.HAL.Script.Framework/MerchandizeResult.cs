namespace Redbox.HAL.Script.Framework;

internal enum MerchandizeResult
{
	Success,
	MachineFull,
	EmptyStuck,
	SlotInUse,
	HardwareError,
	LookupFailure,
	UnloadsCleared,
	TransferFailure,
	QLMFull,
	VMZFull,
	DumpBinFull,
	DiskInVMZ
}
