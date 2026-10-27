using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "ms-take-disk-at-door")]
internal sealed class TakeDiskAtDoorJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			base.Context.CreateInfoResult("HardwareError", $"Read picker sensors returned error {pickerSensorReadResult.Error.ToString()}");
			return;
		}
		if (pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("PickerFull", "The picker is full.");
			return;
		}
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.None, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("HardwareError", $"MOVE failed with error {errorCodes.ToString()}");
			return;
		}
		ErrorCodes errorCodes2 = ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor();
		if (ErrorCodes.PickerFull != errorCodes2)
		{
			base.Context.CreateInfoResult("ReceiveFailure", $"Accept disk returned status {errorCodes2.ToString().ToUpper()}");
			return;
		}
		base.Context.CreateInfoResult("ItemReceived", "Item received in picker.");
		base.ControlSystem.TrackCycle();
	}

	internal TakeDiskAtDoorJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
