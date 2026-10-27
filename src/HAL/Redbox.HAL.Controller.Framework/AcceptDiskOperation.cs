using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class AcceptDiskOperation : AbstractOperation<ErrorCodes>
{
	public override ErrorCodes Execute()
	{
		ErrorCodes errorCodes = OnExecute();
		if ((ErrorCodes.PickerEmpty == errorCodes || ErrorCodes.PickerFull == errorCodes) && !Controller.VendDoorClose().Success)
		{
			errorCodes = ErrorCodes.VendDoorCloseTimeout;
		}
		return errorCodes;
	}

	internal AcceptDiskOperation()
	{
	}

	private ErrorCodes OnExecute()
	{
		LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "[AcceptDiskAtDoor] Start");
		if (OpenDoorChecked() != ErrorCodes.Success)
		{
			return ErrorCodes.VendDoorRentTimeout;
		}
		Controller.StartRollerIn();
		ErrorCodes errorCodes = WaitSensor(PickerInputs.Sensor5, InputState.Active, ControllerConfiguration.Instance.AcceptDiskTimeout);
		if (ErrorCodes.SensorReadError == errorCodes)
		{
			return errorCodes;
		}
		IPickerSensorReadResult pickerSensorReadResult;
		if (ErrorCodes.Timeout == errorCodes)
		{
			Controller.StopRoller();
			pickerSensorReadResult = Controller.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				return ErrorCodes.SensorReadError;
			}
			if (!pickerSensorReadResult.IsFull)
			{
				return ErrorCodes.PickerEmpty;
			}
		}
		LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "[AcceptDiskAtDoor] Sensor 5 or 6 was tripped; pull into the picker.");
		if (!Controller.RollerToPosition(RollerPosition.Position3).Success)
		{
			LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Error, "[AcceptDiskAtDoor] Unable to roll the disk to sensor 3.");
			IPickerSensorReadResult pickerSensorReadResult2 = Controller.ReadPickerSensors();
			pickerSensorReadResult2.Log();
			if (pickerSensorReadResult2.IsFull)
			{
				return ErrorCodes.PickerFull;
			}
			return ErrorCodes.PickerEmpty;
		}
		pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (pickerSensorReadResult.BlockedCount > 3)
		{
			LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "[AcceptDiskAtDoor] the gripper is obstructed.");
			pickerSensorReadResult.Log();
			return ErrorCodes.PickerObstructed;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			return ErrorCodes.PickerEmpty;
		}
		return ErrorCodes.PickerFull;
	}
}
