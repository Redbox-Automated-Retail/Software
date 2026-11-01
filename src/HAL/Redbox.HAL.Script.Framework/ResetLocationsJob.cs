using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-locations")]
internal sealed class ResetLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		int operand = base.Context.PopTop<int>();
		List<ILocation> locations = new List<ILocation>();
		using (new DisposeableList<ILocation>(locations))
		{
			for (int i = 0; i < operand; i++)
			{
				int num2 = base.Context.PopTop<int>();
				int num3 = base.Context.PopTop<int>();
				IDeck byNumber = base.DecksService.GetByNumber(num2);
				if (byNumber != null && byNumber.IsSlotValid(num3))
				{
					ILocation item = inventoryService.Get(num2, num3);
					locations.Add(item);
					base.Context.CreateResult("LocationReset", "The location was reset.", num2, num3, null, null, null);
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
