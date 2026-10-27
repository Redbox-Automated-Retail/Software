using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "soft-sync")]
internal sealed class SoftSyncJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		LogHelper.Instance.WithContext("Begin soft sync request.");
		if (!TestInventoryStore())
		{
			return;
		}
		base.DecksService.ForAllDecksDo(delegate(IDeck deck)
		{
			int numberOfSlots = deck.NumberOfSlots;
			for (int i = 0; i < numberOfSlots; i++)
			{
				int num = i + 1;
				ILocation location = base.InventoryService.Get(deck.Number, num);
				base.Context.CreateResult("SoftSync", "The inventory location has been successfully inspected.", deck.Number, num, location.ID, null, null);
			}
			return true;
		});
		base.Context.CreateInfoResult("SoftSyncEnd", "The soft sync request succeeded.");
	}

	internal SoftSyncJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
