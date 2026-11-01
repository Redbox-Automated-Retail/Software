using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-locations")]
internal sealed class ResetLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		int count = base.Context.PopTop<int>();
		List<ILocation> locations = new List<ILocation>();
		using (new DisposeableList<ILocation>(locations))
		{
			for (int i = 0; i < count; i++)
			{
				int deck = base.Context.PopTop<int>();
				int slot = base.Context.PopTop<int>();
				IDeck byNumber = base.DecksService.GetByNumber(deck);
				if (byNumber != null && byNumber.IsSlotValid(slot))
				{
					ILocation item = inventoryService.Get(deck, slot);
					locations.Add(item);
					base.Context.CreateResult("LocationReset", "The location was reset.", deck, slot, null, null, null);
				}
			}
			inventoryService.Reset(locations);
			base.Context.CreateInfoResult("TotalLocationsReset", locations.Count.ToString());
		}
	}

	internal ResetLocationsJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
