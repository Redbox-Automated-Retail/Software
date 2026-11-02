using System;

namespace Redbox.UpdateService.Client;

public class TransferStatisticReport
{
	public DateTime End { get; set; }

	public double AverageSpeedInKPS { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string ChangeSet { get; set; } = null!;

	public string Repository { get; set; } = null!;

	public string StoreNumber { get; set; } = null!;

	public ulong TotalBytesTransfered { get; set; }

	public bool Success { get; set; }
}
