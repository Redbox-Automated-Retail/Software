using System;

namespace Redbox.UpdateService.Client;

public class TransferStatisticReport
{
	public DateTime End { get; set; }

	public double AverageSpeedInKPS { get; set; }

	public string ChangeSet { get; set; }

	public string Repository { get; set; }

	public string StoreNumber { get; set; }

	public ulong TotalBytesTransfered { get; set; }

	public bool Success { get; set; }
}
