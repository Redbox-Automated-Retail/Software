using System;

namespace Redbox.UpdateServices.Proxy.ComponentModel;

public interface IDownloadData
{
	Guid Id { get; }

	string Name { get; }

	string Owner { get; }

	string Status { get; }

	string JobType { get; }

	ulong TotalBytes { get; }

	DateTime StartTime { get; }

	DateTime? FinishTime { get; }

	DateTime ModifiedTime { get; }

	ulong TotalBytesTransferred { get; }
}
