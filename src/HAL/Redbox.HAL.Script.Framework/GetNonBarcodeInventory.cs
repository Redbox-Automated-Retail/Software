using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-non-barcode-inventory")]
internal sealed class GetNonBarcodeInventory : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<ILocation> unknowns = base.InventoryService.GetUnknowns();
		using (new DisposeableList<ILocation>(unknowns))
		{
			unknowns.ForEach(delegate(ILocation each)
			{
				base.Context.CreateResult("UnknownLocation", "The location is UNKNOWN.", each.Deck, each.Slot, "UNKNOWN", null, null);
			});
		}
		List<ILocation> emptySlots = base.InventoryService.GetEmptySlots();
		using (new DisposeableList<ILocation>(emptySlots))
		{
			emptySlots.ForEach(delegate(ILocation each)
			{
				base.Context.CreateResult("EmptyLocation", "The location is EMPTY.", each.Deck, each.Slot, "EMPTY", null, null);
			});
		}
	}

	internal GetNonBarcodeInventory(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
