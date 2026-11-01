using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class StandardSyncDecorator : SyncDecorator
{
	internal override void OnGetFailure(IGetResult notUsed)
	{
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		SyncLocation.ID = "UNKNOWN";
		inventoryService.Save(SyncLocation);
	}

	internal StandardSyncDecorator(ExecutionContext ctx, ExecutionResult r, ILocation l)
		: base(ctx, r, l)
	{
	}
}
