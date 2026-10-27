using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class MerchTransferManager : IDisposable
{
	internal readonly ExecutionContext Context;

	private bool Disposed;

	private int m_successfulTransfers;

	private int m_transferFailures;

	private readonly bool Debug;

	private readonly List<ITransferResult> Transfers = new List<ITransferResult>();

	internal int SuccessfulTransfers => m_successfulTransfers;

	internal int TransferFailures => m_transferFailures;

	internal MerchandizeResult LastTransfer { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Transfers.Clear();
		}
	}

	internal void ProcessTransfer(ILocation source, ILocation dest)
	{
		Sum(source, dest);
	}

	internal void Process(ITransferResult xferResult)
	{
		if (xferResult.Transferred)
		{
			Transfers.Add(xferResult);
			Sum(xferResult);
			return;
		}
		m_transferFailures++;
		if (xferResult.ReturnedToSource)
		{
			LogHelper.Instance.Log("The transfer failed to put the disk in a target, but it was returned to source.");
			return;
		}
		LogHelper.Instance.Log("The transfer result had a hardware error: {0}", xferResult.TransferError.ToString());
	}

	internal MerchTransferManager(ExecutionContext context, bool debug)
	{
		LastTransfer = MerchandizeResult.Success;
		Debug = debug;
		Context = context;
	}

	private void Sum(ITransferResult xferResult)
	{
		if (xferResult.Transferred)
		{
			Sum(xferResult.Source, xferResult.Destination);
		}
	}

	private void Sum(ILocation source, ILocation dest)
	{
		m_successfulTransfers++;
		Context.CreateResult("DiskTransferSuccessful", SegmentManager.Instance.InCompressedZone(source) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", source.Deck, source.Slot, "EMPTY", null, dest.ID);
		if (ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(dest))
		{
			Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, dest.ID, null, null);
		}
		else
		{
			Context.CreateResult("DiskTransferSuccessful", SegmentManager.Instance.InCompressedZone(dest) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", dest.Deck, dest.Slot, dest.ID, null, "EMPTY");
		}
	}
}
