using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class RejectDiskOperation : AbstractOperation<ErrorCodes>
{
	private readonly int Attempts;

	public override ErrorCodes Execute()
	{
		ErrorCodes errorCodes = OnReject();
		if ((ErrorCodes.PickerEmpty == errorCodes || ErrorCodes.PickerFull == errorCodes) && VendDoorState.Closed != Controller.VendDoorState && !Controller.VendDoorClose().Success)
		{
			errorCodes = ErrorCodes.VendDoorCloseTimeout;
		}
		return errorCodes;
	}

	internal RejectDiskOperation(int attempts)
	{
		Attempts = attempts;
	}

	private ErrorCodes OnReject()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		int attempts = Attempts;
		int ms = 1000;
		if (OpenDoorChecked() != ErrorCodes.Success)
		{
			return ErrorCodes.VendDoorRentTimeout;
		}
		IPickerSensorReadResult pickerSensorReadResult = null;
		while (attempts-- >= 0)
		{
			Controller.TrackCycle();
			pickerSensorReadResult = Controller.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				return ErrorCodes.SensorReadError;
			}
			if (pickerSensorReadResult.BlockedCount == 0)
			{
				if (!Controller.VendDoorClose().Success)
				{
					return ErrorCodes.PickerObstructed;
				}
				return ErrorCodes.PickerEmpty;
			}
			pickerSensorReadResult.Log();
			bool flag = pickerSensorReadResult.IsInputActive(PickerInputs.Sensor4) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor3) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor2) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1);
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor6))
			{
				if (flag)
				{
					ControllerService.PushOut();
					service.Wait(ms);
					continue;
				}
				if (Controller.RollerToPosition(RollerPosition.Position3, 8000).Success)
				{
					pickerSensorReadResult = Controller.ReadPickerSensors();
					if (!pickerSensorReadResult.Success)
					{
						return ErrorCodes.SensorReadError;
					}
					if (!pickerSensorReadResult.IsFull)
					{
						return ErrorCodes.PickerEmpty;
					}
					if (pickerSensorReadResult.BlockedCount <= 3)
					{
						if (!Controller.VendDoorClose().Success)
						{
							return ErrorCodes.PickerObstructed;
						}
						return ErrorCodes.PickerFull;
					}
				}
				service.Wait(ms);
			}
			else if (flag)
			{
				ControllerService.PushOut();
				service.Wait(1500);
			}
		}
		pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			return ErrorCodes.PickerEmpty;
		}
		if (pickerSensorReadResult.BlockedCount > 3)
		{
			return ErrorCodes.PickerObstructed;
		}
		if (!Controller.VendDoorClose().Success)
		{
			return ErrorCodes.PickerObstructed;
		}
		return ErrorCodes.PickerFull;
	}
}
