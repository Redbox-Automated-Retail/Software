using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Operations;

internal sealed class PushOutDiskOperation : AbstractOperation<ErrorCodes>
{
	private readonly IControllerService Service;

	public override ErrorCodes Execute()
	{
		if (CloseTrackChecked() != ErrorCodes.Success)
		{
			return ErrorCodes.TrackCloseTimeout;
		}
		for (int i = 0; i < 3; i++)
		{
			IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				return ErrorCodes.SensorReadError;
			}
			if (!pickerSensorReadResult.IsFull)
			{
				return ErrorCodes.Success;
			}
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor4))
			{
				Controller.StartRollerOut();
				WaitSensor(PickerInputs.Sensor4, InputState.Inactive);
				if (ControllerConfiguration.Instance.PushOutSleepTime2 > 0)
				{
					RuntimeService.Wait(ControllerConfiguration.Instance.PushOutSleepTime2);
				}
				Controller.StopRoller();
				continue;
			}
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor2) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor3))
			{
				if (!Controller.RollerToPosition(RollerPosition.Position4).Success)
				{
					Controller.LogPickerSensorState(LogEntryType.Error);
					continue;
				}
				RuntimeService.SpinWait(50);
				Controller.StartRollerOut();
				WaitSensor(PickerInputs.Sensor4, InputState.Inactive);
				Controller.StopRoller();
				continue;
			}
			return ErrorCodes.Success;
		}
		IPickerSensorReadResult pickerSensorReadResult2 = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult2.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult2.IsInputActive(PickerInputs.Sensor1) && !pickerSensorReadResult2.IsInputActive(PickerInputs.Sensor2) && !pickerSensorReadResult2.IsInputActive(PickerInputs.Sensor3) && !pickerSensorReadResult2.IsInputActive(PickerInputs.Sensor4))
		{
			return ErrorCodes.Success;
		}
		return ErrorCodes.PickerFull;
	}

	internal PushOutDiskOperation(IControllerService cs)
	{
		Service = cs;
	}
}
