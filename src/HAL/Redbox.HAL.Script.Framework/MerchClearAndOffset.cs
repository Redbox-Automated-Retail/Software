using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "merch-clear-and-offset")]
internal sealed class MerchClearAndOffset : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int deck = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = service.Get(deck, slot);
		bool flag = service.Reset(location);
		base.Context.CreateInfoResult(flag ? "ResetSuccess" : "ResetFailure", "The location was {0} reset.", flag ? "successfully" : "unsuccessfully");
		ShowEmptyStuck(location);
	}

	internal MerchClearAndOffset(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
