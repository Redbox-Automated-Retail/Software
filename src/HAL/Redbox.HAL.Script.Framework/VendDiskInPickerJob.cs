using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "ms-vend-disk-in-picker")]
internal sealed class VendDiskInPickerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			base.Context.CreateInfoResult("HardwareError", $"Read picker sensors returned error {pickerSensorReadResult.Error}");
			return;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("EmptyPicker", "No disk in picker.");
			return;
		}
		ErrorCodes errorCodes = base.MotionService.MoveVend(MoveMode.None, base.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("HardwareError", $"MOVEVEND returned error {errorCodes.ToString().ToUpper()}");
			return;
		}
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		IVendItemResult vendItemResult = service.VendItemInPicker();
		if (ErrorCodes.PickerEmpty == vendItemResult.Status)
		{
			base.Context.CreateInfoResult("ItemVended", "The item was vended.");
			return;
		}
		if (ErrorCodes.PickerFull != vendItemResult.Status)
		{
			base.Context.CreateInfoResult("HardwareError", vendItemResult.Presented ? $"VendItem returned {vendItemResult.Status.ToString().ToUpper()}" : "The disk was not presented.");
			return;
		}
		ErrorCodes errorCodes2 = service.AcceptDiskAtDoor();
		if (ErrorCodes.PickerFull != errorCodes2)
		{
			base.Context.CreateInfoResult("TakeDiskError", $"Take disk returned error {errorCodes2.ToString().ToUpper()}");
		}
	}

	internal VendDiskInPickerJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
