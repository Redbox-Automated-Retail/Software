using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-excluded-locations", Operand = "GET-EXCLUDED-LOCS")]
internal sealed class GetExcludedLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<ILocation> excludedSlots = base.InventoryService.GetExcludedSlots();
		using (new DisposeableList<ILocation>(excludedSlots))
		{
			if (excludedSlots.Count == 0)
			{
				base.Context.CreateInfoResult("NoExcludedSlots", "There are no excluded slots.");
				return;
			}
			excludedSlots.ForEach(delegate(ILocation loc)
			{
				base.Context.CreateResult("ExcludedSlot", "The location is excluded.", loc.Deck, loc.Slot, null, null, null);
			});
		}
	}

	internal GetExcludedLocationsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
