using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "read-fraud-disc", Operand = "READ-FRAUD-DISK")]
internal sealed class ReadFraudDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: false, "Fraud");
		applicationLog.Write("Read Fraud Disc Start.");
		if (base.MotionService.MoveVend(MoveMode.Get, applicationLog) != ErrorCodes.Success)
		{
			EndJob(errored: true);
			return;
		}
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor();
		if (ErrorCodes.PickerFull != errorCodes)
		{
			EndJob(errored: true);
			return;
		}
		base.ControlSystem.TrackCycle();
		bool errored = true;
		using (Scanner scanner = new Scanner(base.ControlSystem))
		{
			using ScanResult scanResult = scanner.Read();
			if (scanResult.ReadCode)
			{
				base.Context.CreateInfoResult("FraudItem", "This is the barcode to be marked as fraud.", scanResult.ScannedMatrix);
				errored = false;
			}
		}
		EndJob(errored);
	}

	internal ReadFraudDiskJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void EndJob(bool errored)
	{
		try
		{
			if (errored)
			{
				base.Context.CreateInfoResult("HardwareError", "A hardware error occurred.", "UNKNOWN");
			}
			IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				base.Context.CreateInfoResult("HardwareError", $"Read picker sensors returned error {pickerSensorReadResult.Error}", "UNKNOWN");
				return;
			}
			if (pickerSensorReadResult.IsFull)
			{
				ServiceLocator.Instance.GetService<IControllerService>().VendItemInPicker();
			}
			base.ControlSystem.VendDoorClose();
		}
		finally
		{
			base.AppLog.Write("Read fraud disk end.");
		}
	}
}
