using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct CleanResult
{
	internal bool VisitedAllSlots { get; set; }

	internal int Transfers { get; private set; }

	internal bool NoEmptySlots { get; set; }

	internal int TransferFailures { get; private set; }

	internal void Accumulate(MerchTransferManager manager)
	{
		Transfers += manager.SuccessfulTransfers;
		TransferFailures += manager.TransferFailures;
	}

	internal void Accumulate(ITransferResult result)
	{
		if (result.Transferred)
		{
			Transfers++;
		}
		else
		{
			TransferFailures++;
		}
	}

	internal void Process(MerchandizeResult result)
	{
		if (MerchandizeResult.EmptyStuck != result && result != MerchandizeResult.Success)
		{
			int transferFailures = TransferFailures + 1;
			TransferFailures = transferFailures;
		}
	}
}
