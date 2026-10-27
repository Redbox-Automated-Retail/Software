using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-barcode-vmz-location", Operand = "GET-BARCODE-VMZ-LOCATION")]
internal sealed class GetDiskLocationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		for (int i = 0; i < num; i++)
		{
			string text = base.Context.PopTop<string>();
			ILocation location = base.InventoryService.Lookup(text);
			if (location == null)
			{
				base.Context.CreateInfoResult("BarcodeNotInInventory", "The barcode isn't in inventory.");
				continue;
			}
			bool flag = SegmentManager.Instance.InCompressedZone(location);
			base.Context.CreateResult(flag ? "BarcodeInVMZ" : "BarcodeOutsideVMZ", "Barcode status", location.Deck, location.Slot, text, null, null);
		}
	}

	internal GetDiskLocationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
