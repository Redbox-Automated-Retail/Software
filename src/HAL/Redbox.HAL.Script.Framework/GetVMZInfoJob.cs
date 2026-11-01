using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vmz-merch-summary", Operand = "VMZ-MERCH-SUMMARY")]
internal sealed class GetVMZInfoJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ILocation location = null;
		foreach (MerchFlags flags in SegmentManager.Instance.DecreasingPriorityOrder)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(flags);
			location = merchSegment.FindHigh();
			if (location == null)
			{
				base.Context.CreateInfoResult($"No{flags.ToString()}s", $"There are no {flags.ToString()} in the VMZ.");
				continue;
			}
			string code = $"VMZ{flags.ToString()}Count";
			int itemCount = merchSegment.ItemCount();
			base.Context.CreateInfoResult(code, itemCount.ToString());
			string lastInfoCode = $"Last{flags.ToString()}Info";
			base.Context.CreateResult(lastInfoCode, $"The high {flags.ToString()} was found.", location.Deck, location.Slot, location.ID, null, null);
		}
		base.Context.CreateInfoResult("DumpBinInventoryCount", base.DumpbinService.CurrentCount().ToString());
	}

	internal GetVMZInfoJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
