using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "merch-clear-and-offset")]
internal sealed class MerchClearAndOffset : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int deck = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = inventoryService.Get(deck, slot);
		bool flag = inventoryService.Reset(location);
		base.Context.CreateInfoResult(flag ? "ResetSuccess" : "ResetFailure", "The location was {0} reset.", flag ? "successfully" : "unsuccessfully");
		ShowEmptyStuck(location);
	}

	internal MerchClearAndOffset(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
