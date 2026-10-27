namespace Redbox.UpdateManager.ComponentModel;

public enum TransferStatus : byte
{
	Queued,
	Connecting,
	Transfering,
	Suspended,
	Error,
	TransientError,
	Transferred,
	Acknowledged,
	Cancelled
}
