using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class VMZThinHelper : ThinHelper
{
	protected override ILocation FindEmptyTarget(MerchFlags flags, out MerchandizeResult result)
	{
		LogHelper.Instance.Log(LogEntryType.Debug, "[FindEmptyTarget] Context = {0}", flags.ToString());
		if (ServiceLocator.Instance.GetService<IInventoryService>().MachineIsFull())
		{
			result = MerchandizeResult.MachineFull;
			return null;
		}
		result = MerchandizeResult.Success;
		using MerchTransferManager manager = new MerchTransferManager(base.Context, ThinHelper.Debug);
		ILocation location = SegmentManager.Instance.SelectLocation(flags, manager, out result);
		if (location != null)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("[FindEmptyTarget] Select location found target at = {0}", location.ToString());
			}
			return location;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("[FindEmptyTarget] Select location didn't find a target; return code = {0}", result.ToString());
		}
		if (result != MerchandizeResult.VMZFull && result != MerchandizeResult.Success)
		{
			return null;
		}
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (service.IsFull())
		{
			LogHelper.Instance.Log("[FindEmptyTarget] MoveLowPriorityToBin: the dump bin is full.");
			result = MerchandizeResult.VMZFull;
			return null;
		}
		ILocation location2 = null;
		foreach (MerchFlags item in SegmentManager.Instance.IncreasingPriorityOrder)
		{
			if (item == flags)
			{
				break;
			}
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			if (!merchSegment.CanDump)
			{
				continue;
			}
			ILocation location3 = merchSegment.FindLow();
			if (location3 == null)
			{
				continue;
			}
			string iD = location3.ID;
			MerchFlags flags2 = location3.Flags;
			if (FetchThin(location3, iD) != MerchandizeResult.Success)
			{
				LogHelper.Instance.Log("Thin: cannot move low at {0}; bail.", location3.ToString());
				continue;
			}
			ILocation location4 = ((location2 == null) ? service.PutLocation : location2);
			MerchandizeResult merchandizeResult = ThinDiskToLocation(location3, location4, iD, flags2);
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("MoveLowPriorityToBin: transfer of {0} [Flags = {1}] to {2} returned {3}", location3.ToString(), item.ToString(), location4.ToString(), merchandizeResult.ToString());
			}
			if (merchandizeResult != MerchandizeResult.Success)
			{
				result = merchandizeResult;
				return null;
			}
			location2 = location3;
		}
		if (location2 != null)
		{
			result = MerchandizeResult.Success;
			return location2;
		}
		result = MerchandizeResult.VMZFull;
		return null;
	}

	internal VMZThinHelper(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	private ILocation TryDumpBin(MerchFlags flags, out MerchandizeResult result)
	{
		return TryDumpBin(MerchSegmentFactory.Get(flags), out result);
	}

	private ILocation TryDumpBin(MerchSegment segment, out MerchandizeResult result)
	{
		if (!segment.CanDump)
		{
			LogHelper.Instance.Log("There are insufficient slots in the machine.");
			result = MerchandizeResult.MachineFull;
			return null;
		}
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (service.IsFull())
		{
			base.Context.CreateInfoResult("DumpBinIsFull", "The dump bin is filled to capacity.");
			result = MerchandizeResult.MachineFull;
			return null;
		}
		result = MerchandizeResult.Success;
		return service.PutLocation;
	}
}
