using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class VendItemOperation : AbstractOperation<VendItemResult>
{
	private readonly int PollCount;

	public override VendItemResult Execute()
	{
		VendItemResult vendItemResult = OnExecute();
		if (ErrorCodes.PickerEmpty == vendItemResult.Status && !Controller.VendDoorClose().Success)
		{
			vendItemResult.Status = ErrorCodes.VendDoorCloseTimeout;
		}
		return vendItemResult;
	}

	internal VendItemOperation(int tries)
	{
		PollCount = tries;
	}

	private VendItemResult OnExecute()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		VendItemResult vendItemResult = new VendItemResult
		{
			Presented = true,
			Status = ErrorCodes.PickerEmpty
		};
		LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "VendDiskAtDoor...");
		Controller.RollerToPosition(RollerPosition.Position5, 3000);
		ErrorCodes errorCodes = OpenDoorChecked();
		if (errorCodes != ErrorCodes.Success)
		{
			vendItemResult.Status = errorCodes;
			return vendItemResult;
		}
		int ms = 1000;
		ErrorCodes errorCodes2 = ControllerService.PushOut();
		if (ErrorCodes.TrackCloseTimeout == errorCodes2 || ErrorCodes.SensorReadError == errorCodes2)
		{
			vendItemResult.Status = errorCodes2;
			vendItemResult.Presented = false;
			return vendItemResult;
		}
		Controller.TrackCycle();
		int num = 0;
		IPickerSensorReadResult pickerSensorReadResult;
		for (int i = 0; i <= PollCount; i++)
		{
			pickerSensorReadResult = Controller.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				vendItemResult.Status = ErrorCodes.SensorReadError;
				return vendItemResult;
			}
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor2) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor3) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor4))
			{
				pickerSensorReadResult.Log(LogEntryType.Error);
				if (num == 1 && ControllerConfiguration.Instance.TrackPushOutFailures)
				{
					vendItemResult.Presented = false;
					LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Error, "[VendItemInPicker] After 2 pushes, the disk has not cleared sensor 4.");
					vendItemResult.Status = ErrorCodes.PickerFull;
					return vendItemResult;
				}
				num++;
				LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Error, "[VendItemInPicker] Disc was pushed to the drum - push it back out.");
				Controller.TrackCycle();
				ControllerService.PushOut();
				service.Wait(ms);
				continue;
			}
			num = 0;
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor5))
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "[VendItemInPicker] The item is still blocking sensor 5.");
				}
				service.Wait(ms);
				continue;
			}
			if (!pickerSensorReadResult.IsInputActive(PickerInputs.Sensor6))
			{
				vendItemResult.Status = ErrorCodes.PickerEmpty;
				return vendItemResult;
			}
			service.Wait(ms);
		}
		pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			vendItemResult.Status = ErrorCodes.SensorReadError;
		}
		else
		{
			vendItemResult.Status = (pickerSensorReadResult.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty);
		}
		return vendItemResult;
	}
}
