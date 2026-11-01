using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "prep-deck-for-redeploy", Operand = "PREP-DECK-FOR-REDEPLOY")]
internal sealed class PrepDeckForRedeployJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int deckCount = base.Context.PopTop<int>();
		int changedCount = 0;
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		for (int i = 0; i < deckCount; i++)
		{
			int deck = base.Context.PopTop<int>();
			IDeck byNumber = base.DecksService.GetByNumber(deck);
			if (byNumber == null)
			{
				LogHelper.Instance.Log("Invalid deck '{0}' specified to prep redeploy deck job.", deck);
				continue;
			}
			if (!isVMZMachine && byNumber.IsQlm)
			{
				LogHelper.Instance.Log("Deck specified '{0}' is QLM; will not alter.", deck);
				continue;
			}
			Range slotRange = new Range((!isVMZMachine) ? 1 : (VMZ.Instance.Bounds.End + 1), byNumber.NumberOfSlots);
			List<int> changedSlots = base.InventoryService.SwapEmptyWith(byNumber, "UNKNOWN", MerchFlags.None, slotRange);
			foreach (int slot in changedSlots)
			{
				base.Context.CreateResult("LocationChanged", "The location was set to UNKNOWN", deck, slot, null, null, null);
			}
			changedCount += changedSlots.Count;
			LogHelper.Instance.Log("Inventory: changed {0} empty slots on deck {1} to UNKNOWN", changedSlots.Count, deck);
			changedSlots.Clear();
		}
		base.Context.CreateInfoResult("LocationChangedCount", changedCount.ToString());
	}

	internal PrepDeckForRedeployJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
