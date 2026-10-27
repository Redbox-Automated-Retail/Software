using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel;

public interface ITransferJob
{
	TransferJobType JobType { get; }

	string Name { get; }

	Guid ID { get; }

	DateTime StartTime { get; }

	DateTime ModifiedTime { get; }

	DateTime? FinishTime { get; }

	string Owner { get; }

	ulong TotalBytesTransfered { get; }

	ulong TotalBytes { get; }

	TransferStatus Status { get; }

	ErrorList AddItem(string url, string file);

	ErrorList Complete();

	ErrorList Cancel();

	ErrorList Suspend();

	ErrorList Resume();

	ErrorList SetPriority(TransferJobPriority priority);

	ErrorList SetCallback(ITransferCallbackParameters parameters);

	ErrorList GetItems(out List<ITransferItem> items);

	ErrorList TakeOwnership();

	ErrorList SetNoProgressTimeout(uint period);

	ErrorList SetMinimumRetryDelay(uint seconds);

	void GetErrors(out ErrorList errors);
}
