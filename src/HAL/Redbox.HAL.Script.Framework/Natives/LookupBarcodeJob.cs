using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "lookup-barcodes")]
internal sealed class LookupBarcodeJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int count = base.Context.PopTop<int>();
		string[] barcodes = new string[count];
		for (int i = 0; i < count; i++)
		{
			barcodes[i] = base.Context.PopTop<string>();
		}
		IInventoryService iis = ServiceLocator.Instance.GetService<IInventoryService>();
		Array.ForEach(barcodes, delegate(string id)
		{
			ILocation location = iis.Lookup(id);
			if (location == null)
			{
				base.Context.CreateInfoResult("BarcodeNotFound", $"The barcode {id} was not found in inventory", id);
			}
			else
			{
				base.Context.CreateResult("BarcodeFound", $"The barcode {id} was found in inventory", location.Deck, location.Slot, id, null, null);
			}
		});
	}

	internal LookupBarcodeJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
