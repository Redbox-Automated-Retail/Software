using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "mark-barcodes-unknown")]
internal sealed class MarkBarcodesUnknownJobs : NativeJobAdapter
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
				string id = base.Context.PopTop<string>();
				ILocation location = service.Lookup(id);
				if (location != null)
				{
					location.ID = "UNKNOWN";
				}
			}
			if (list.Count > 0 && !service.Save(list))
			{
				AddError("FAILED TO UPDATE");
			}
		}
	}

	internal MarkBarcodesUnknownJobs(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
