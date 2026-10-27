namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface ISnapshotResult
{
	bool Success { get; }

	int StatusCode { get; }

	Error Error { get; }
}
