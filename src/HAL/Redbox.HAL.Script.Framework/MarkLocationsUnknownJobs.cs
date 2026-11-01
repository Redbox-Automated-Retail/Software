using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "mark-locations-unknown")]
internal sealed class MarkLocationsUnknownJobs : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int operand = base.Context.PopTop<int>();
		if (operand == 0)
		{
			return;
		}
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			for (int i = 0; i < operand; i++)
			{
				int deck = base.Context.PopTop<int>();
				int slot = base.Context.PopTop<int>();
				ILocation location = inventoryService.Get(deck, slot);
				if (location != null)
				{
					location.ID = "UNKNOWN";
				}
			}
			if (!inventoryService.Save(list))
			{
				AddError("FAILED TO UPDATE");
			}
		}
	}

	internal MarkLocationsUnknownJobs(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
