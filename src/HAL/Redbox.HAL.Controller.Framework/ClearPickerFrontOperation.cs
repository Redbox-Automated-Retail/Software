using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public sealed class ClearPickerFrontOperation : AbstractOperation<bool>
{
	public override bool Execute()
	{
		if (!Controller.SetFinger(GripperFingerState.Rent).Success)
		{
			return false;
		}
		if (!Controller.RetractArm().Success)
		{
			Controller.SetFinger(GripperFingerState.Rent);
			if (!Controller.RetractArm().Success)
			{
				return false;
			}
		}
		if (!Controller.SetFinger(GripperFingerState.Closed).Success)
		{
			return false;
		}
		Controller.TimedExtend(ControllerConfiguration.Instance.PushTime);
		if (!Controller.SetFinger(GripperFingerState.Rent).Success)
		{
			return false;
		}
		if (!Controller.RetractArm().Success)
		{
			return false;
		}
		if (!Controller.SetFinger(GripperFingerState.Closed).Success)
		{
			return false;
		}
		Controller.TimedExtend(ControllerConfiguration.Instance.PushTime);
		if (!Controller.RetractArm().Success)
		{
			return false;
		}
		return Controller.SetFinger(GripperFingerState.Rent).Success;
	}
}
