using System;
using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.Transfer;

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

	List<Error> AddItem(string url, string file);

	List<Error> Complete();

	List<Error> Cancel();

	List<Error> Suspend();

	List<Error> Resume();

	List<Error> SetPriority(TransferJobPriority priority);

	List<Error> SetCallback(ITransferCallbackParameters parameters);

	List<Error> GetItems(out List<ITransferItem> items);

	List<Error> TakeOwnership();

	List<Error> SetNoProgressTimeout(uint period);

	List<Error> SetMinimumRetryDelay(uint seconds);

	List<Error> GetErrors();
}
