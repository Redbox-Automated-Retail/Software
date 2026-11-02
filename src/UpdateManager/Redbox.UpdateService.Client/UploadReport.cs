using System;

namespace Redbox.UpdateService.Client;

public class UploadReport
{
	public DateTime End { get; set; }

	public double AverageSpeedInKPS { get; set; }

	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public ulong TotalBytesTransfered { get; set; }

	public bool Success { get; set; }
}
