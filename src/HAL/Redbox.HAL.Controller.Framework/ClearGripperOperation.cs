using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ClearGripperOperation : AbstractOperation<ErrorCodes>
{
	public override ErrorCodes Execute()
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		ErrorCodes errorCodes = CloseTrackChecked();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		for (int i = 0; i < 2; i++)
		{
			IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				return pickerSensorReadResult.Error;
			}
			if (!pickerSensorReadResult.IsFull)
			{
				return ErrorCodes.Success;
			}
			if (!pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1))
			{
				if (!pickerSensorReadResult.IsInputActive(PickerInputs.Sensor6))
				{
					return ErrorCodes.Success;
				}
				Controller.SetFinger(GripperFingerState.Rent);
				try
				{
					if (service.AtVendDoor && VendDoorState.Closed != Controller.VendDoorState)
					{
						Controller.VendDoorRent();
					}
					Controller.RollerToPosition(RollerPosition.Position3, 3000);
				}
				finally
				{
					if (VendDoorState.Closed != Controller.VendDoorState)
					{
						Controller.VendDoorClose();
					}
				}
				continue;
			}
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor6))
			{
				return ErrorCodes.PickerObstructed;
			}
			if (ClearPickerBlockedCount(pickerSensorReadResult) == 0)
			{
				try
				{
					Controller.StartRollerIn();
					PushIntoSlot();
				}
				finally
				{
					Controller.StopRoller();
				}
			}
			else
			{
				Controller.SetFinger(GripperFingerState.Rent);
				Controller.RollerToPosition(RollerPosition.Position5);
			}
		}
		return ErrorCodes.PickerObstructed;
	}
}
