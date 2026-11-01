using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-barcode-vmz-location", Operand = "GET-BARCODE-VMZ-LOCATION")]
internal sealed class GetDiskLocationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int count = base.Context.PopTop<int>();
		for (int i = 0; i < count; i++)
		{
			string barcode = base.Context.PopTop<string>();
			ILocation location = base.InventoryService.Lookup(barcode);
			if (location == null)
			{
				base.Context.CreateInfoResult("BarcodeNotInInventory", "The barcode isn't in inventory.");
				continue;
			}
			bool inVmz = SegmentManager.Instance.InCompressedZone(location);
			base.Context.CreateResult(inVmz ? "BarcodeInVMZ" : "BarcodeOutsideVMZ", "Barcode status", location.Deck, location.Slot, barcode, null, null);
		}
	}

	internal GetDiskLocationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
