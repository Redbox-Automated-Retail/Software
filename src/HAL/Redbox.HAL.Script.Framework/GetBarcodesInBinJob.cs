using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-barcodes-in-bin", Operand = "GET-BARCODES-IN-BIN")]
internal sealed class GetBarcodesInBinJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IList<IDumpBinInventoryItem> barcodesInBin = ServiceLocator.Instance.GetService<IDumpbinService>().GetBarcodesInBin();
		using (new DisposeableList<IDumpBinInventoryItem>(barcodesInBin))
		{
			foreach (IDumpBinInventoryItem item in barcodesInBin)
			{
				base.Context.CreateInfoResult("DumpBinInventoryItem", "The item is in the dump bin.", item.ID);
			}
		}
	}

	internal GetBarcodesInBinJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
