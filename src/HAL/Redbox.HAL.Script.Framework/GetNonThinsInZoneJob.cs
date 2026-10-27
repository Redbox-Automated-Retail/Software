using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-nonthins-in-vmz", Operand = "GET-NONTHINS-IN-VMZ")]
internal sealed class GetNonThinsInZoneJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<ILocation> nonThinItemsInCompressedZone = SegmentManager.Instance.GetNonThinItemsInCompressedZone();
		using (new DisposeableList<ILocation>(nonThinItemsInCompressedZone))
		{
			foreach (ILocation item in nonThinItemsInCompressedZone)
			{
				base.Context.CreateResult("NonThinInVMZ", "The item is a non-thin in the VMZ.", item.Deck, item.Slot, item.ID, null, null);
			}
			base.Context.CreateInfoResult("NonThinInVMZCount", nonThinItemsInCompressedZone.Count.ToString());
		}
	}

	internal GetNonThinsInZoneJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
