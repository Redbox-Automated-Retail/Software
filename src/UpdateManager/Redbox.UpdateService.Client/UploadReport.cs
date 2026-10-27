using System;

namespace Redbox.UpdateService.Client;

public class UploadReport
{
	public DateTime End { get; set; }

	public double AverageSpeedInKPS { get; set; }

	public long ID { get; set; }

	public string StoreNumber { get; set; }

	public ulong TotalBytesTransfered { get; set; }

	public bool Success { get; set; }
}
