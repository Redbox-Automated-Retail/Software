using System;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Client;

public class Download
{
	public TransferJobType JobType { get; set; }

	public string Owner { get; set; }

	public string Name { get; set; }

	public Guid ID { get; set; }

	public DateTime StartTime { get; set; }

	public DateTime ModifiedTime { get; set; }

	public DateTime? FinishTime { get; set; }

	public ulong TotalBytesTransfered { get; set; }

	public ulong TotalBytes { get; set; }

	public TransferStatus Status { get; set; }
}
