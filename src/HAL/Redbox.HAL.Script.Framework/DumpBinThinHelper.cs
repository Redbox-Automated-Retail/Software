using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class DumpBinThinHelper : ThinHelper
{
	internal DumpBinThinHelper(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	internal override MerchandizeResult ThinDisk(string id, MerchFlags flags)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service.IsFull())
		{
			base.Context.CreateInfoResult("DumpBinIsFull", "The dump bin is filled to capacity.");
			return MerchandizeResult.DumpBinFull;
		}
		base.Context.AppLog.Write($"Attempt thin of item {id}");
		ILocation location = service2.Lookup(id);
		if (location == null)
		{
			base.Context.CreateLookupErrorResult("ThinLookupFailed", id);
			return MerchandizeResult.LookupFailure;
		}
		MerchandizeResult merchandizeResult = FetchThin(location, id);
		switch (merchandizeResult)
		{
		case MerchandizeResult.EmptyStuck:
			return merchandizeResult;
		case MerchandizeResult.Success:
			return ThinDiskToLocation(location, service.PutLocation, id, MerchFlags.Thin);
		default:
		{
			MerchandizeResult merchandizeResult2 = ReturnToSource(location, id);
			if (merchandizeResult2 != MerchandizeResult.Success)
			{
				return merchandizeResult2;
			}
			return merchandizeResult;
		}
		}
	}

	protected override ILocation FindEmptyTarget(MerchFlags flags, out MerchandizeResult result)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (service.IsFull())
		{
			result = MerchandizeResult.DumpBinFull;
			return null;
		}
		result = MerchandizeResult.Success;
		return service.PutLocation;
	}
}
