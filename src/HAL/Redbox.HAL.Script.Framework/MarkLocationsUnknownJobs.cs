using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "mark-locations-unknown")]
internal sealed class MarkLocationsUnknownJobs : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		if (num == 0)
		{
			return;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			for (int i = 0; i < num; i++)
			{
				int deck = base.Context.PopTop<int>();
				int slot = base.Context.PopTop<int>();
				ILocation location = service.Get(deck, slot);
				if (location != null)
				{
					location.ID = "UNKNOWN";
				}
			}
			if (!service.Save(list))
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
