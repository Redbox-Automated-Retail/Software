using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "prep-deck-for-redeploy", Operand = "PREP-DECK-FOR-REDEPLOY")]
internal sealed class PrepDeckForRedeployJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		int num2 = 0;
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		for (int i = 0; i < num; i++)
		{
			int num3 = base.Context.PopTop<int>();
			IDeck byNumber = base.DecksService.GetByNumber(num3);
			if (byNumber == null)
			{
				LogHelper.Instance.Log("Invalid deck '{0}' specified to prep redeploy deck job.", num3);
				continue;
			}
			if (!isVMZMachine && byNumber.IsQlm)
			{
				LogHelper.Instance.Log("Deck specified '{0}' is QLM; will not alter.", num3);
				continue;
			}
			Range slotRange = new Range((!isVMZMachine) ? 1 : (VMZ.Instance.Bounds.End + 1), byNumber.NumberOfSlots);
			List<int> list = base.InventoryService.SwapEmptyWith(byNumber, "UNKNOWN", MerchFlags.None, slotRange);
			foreach (int item in list)
			{
				base.Context.CreateResult("LocationChanged", "The location was set to UNKNOWN", num3, item, null, null, null);
			}
			num2 += list.Count;
			LogHelper.Instance.Log("Inventory: changed {0} empty slots on deck {1} to UNKNOWN", list.Count, num3);
			list.Clear();
		}
		base.Context.CreateInfoResult("LocationChangedCount", num2.ToString());
	}

	internal PrepDeckForRedeployJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
