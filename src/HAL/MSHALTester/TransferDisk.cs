using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class TransferDisk
{
	private readonly HardwareService Service;

	internal string Transfer(TransferLocation source, TransferLocation target)
	{
		if (!Service.ExecuteImmediate($"XFER SRC-DECK={source.Deck} SRC-SLOT={source.Slot} DEST-DECK={target.Deck} DEST-SLOT={target.Slot}", 120000, out var job).Success)
		{
			return "SERVICE COMM ERROR";
		}
		string topOfStack = job.GetTopOfStack();
		if (!string.IsNullOrEmpty(topOfStack))
		{
			return topOfStack;
		}
		return "SERVICE COMM ERROR";
	}

	internal TransferDisk(HardwareService service)
	{
		Service = service;
	}
}
