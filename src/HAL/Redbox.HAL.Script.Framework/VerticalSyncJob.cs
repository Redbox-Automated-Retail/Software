using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vertical-sync", Operand = "VERTSYNC")]
internal sealed class VerticalSyncJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int errors = 0;
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "Sync", flushOnWrite: true, null);
		IControllerService cs = ServiceLocator.Instance.GetService<IControllerService>();
		int slot = base.Context.PopTop<int>();
		applicationLog.Write($"Start vertical sync for slot {slot}");
		base.DecksService.ForAllDecksDo(delegate(IDeck deck)
		{
			if (deck.IsQlm)
			{
				return true;
			}
			ILocation location = base.InventoryService.Get(deck.Number, slot);
			ITransferResult transferResult = cs.Transfer(location, location);
			if (!transferResult.Transferred)
			{
				HandleError(deck.Number, slot, $"Transfer failed with error {transferResult.TransferError}");
				int num = errors + 1;
				errors = num;
				return false;
			}
			return true;
		});
		if (errors == 0)
		{
			applicationLog.Write("The sync completed successfully.");
			base.Context.CreateInfoResult("SyncSuccess", "The test succeeded.");
			base.MotionService.MoveVend(MoveMode.Get, applicationLog);
		}
	}

	internal VerticalSyncJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void HandleError(int deck, int slot, string msg)
	{
		base.Context.CreateResult("SyncFailure", msg, deck, slot, null, null, null);
		AddError("Job error.");
	}
}
