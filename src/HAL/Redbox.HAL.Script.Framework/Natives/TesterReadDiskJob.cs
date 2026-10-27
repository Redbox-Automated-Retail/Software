using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "read-barcode")]
internal sealed class TesterReadDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			AddError("Picker sensor read error");
			return;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("NoDisk", "The picker is empty.");
			AddError("Picker empty");
			return;
		}
		using Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, base.ControlSystem);
		using ScanResult scanResult = scanner.Read();
		scanResult.PushTo(base.Context);
		if (scanResult.ReadCode && base.InventoryService.IsBarcodeDuplicate(scanResult.ScannedMatrix, out var _))
		{
			base.Context.CreateDuplicateItemResult(scanResult.ScannedMatrix);
		}
	}

	internal TesterReadDiskJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
