using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class CleanManager
{
	internal static CleanManager Instance => Singleton<CleanManager>.Instance;

	internal void CleanSlot(ref CleanResult result, ExecutionContext context, SegmentManager manager)
	{
		MerchTransferManager merchTransferManager = new MerchTransferManager(context, ThinHelper.Debug);
		try
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			using VMZIterator vMZIterator = new VMZIterator();
			_ = context.AppLog;
			do
			{
				if (vMZIterator.IsEmpty)
				{
					if (!CompressToEmpty(vMZIterator, manager, merchTransferManager, ref result))
					{
						return;
					}
				}
				else if (service.IsStuck(vMZIterator.Location))
				{
					LogHelper.Instance.WithContext("CleanManager: don't process location {0} stuckFlags {1}", vMZIterator.Location.ToString(), vMZIterator.Location.StuckCount.ToString());
				}
				else if (manager.IsNonThinType(vMZIterator.Flags))
				{
					MerchandizeResult merchandizeResult = manager.MoveNonThin(vMZIterator.Location, vMZIterator.Location, merchTransferManager);
					if (merchandizeResult == MerchandizeResult.Success)
					{
						CompressToEmpty(vMZIterator, manager, merchTransferManager, ref result);
						return;
					}
					if (MerchandizeResult.MachineFull == merchandizeResult)
					{
						result.VisitedAllSlots = true;
						return;
					}
					if (MerchandizeResult.EmptyStuck != merchandizeResult)
					{
						result.Process(merchandizeResult);
						return;
					}
				}
			}
			while (vMZIterator.Up());
			result.VisitedAllSlots = true;
		}
		finally
		{
			result.Accumulate(merchTransferManager);
		}
	}

	private bool CompressToEmpty(VMZIterator iterator, SegmentManager manager, MerchTransferManager xferMgr, ref CleanResult result)
	{
		MerchSegment merchSegment = manager.FindContainingOrHigher(iterator);
		if (merchSegment == null)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("[CleanSlot] Did not find an enclosing segment for location {0}", iterator.Location.ToString());
			}
			return true;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("[CleanSlot] Found enclosing segment [flags = {0}] for location {1}", merchSegment.Flags.ToString(), iterator.Location.ToString());
		}
		ILocation location = merchSegment.FindHigh();
		if (ServiceLocator.Instance.GetService<IInventoryService>().IsStuck(location))
		{
			return true;
		}
		MerchandizeResult merchandizeResult = manager.MoveThin(location, iterator.Location, xferMgr);
		result.Process(merchandizeResult);
		return MerchandizeResult.EmptyStuck == merchandizeResult;
	}

	private CleanManager()
	{
	}
}
