using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "ms-pull-in-dvd")]
internal sealed class MSPullInDVDJob : NativeJobAdapter
{
	private string ErrorCode = "SUCCESS";

	protected override void ExecuteInner()
	{
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			ErrorCode = "PICKERSENSORREADFAILURE";
			return;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("InvalidJobUse", "Picker is clear.");
			AddError("pull-in-dvd failed.");
			return;
		}
		try
		{
			IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
			GripperFingerState finger = GripperFingerState.Rent;
			ILocation currentLocation = service.CurrentLocation;
			if (currentLocation != null && ServiceLocator.Instance.GetService<IDecksService>().GetFrom(currentLocation).IsSlotSellThru(currentLocation.Slot))
			{
				finger = GripperFingerState.Open;
			}
			if (!base.ControlSystem.TrackOpen().Success)
			{
				ErrorCode = "TRACK-OPEN-TIMEOUT";
				return;
			}
			base.ControlSystem.StartRollerOut();
			for (int i = 0; i < ControllerConfiguration.Instance.NumberOfPulls; i++)
			{
				if (!base.ControlSystem.SetFinger(finger).Success)
				{
					ErrorCode = "GRIPPER FINGER TIMEOUT";
					return;
				}
				if (!base.ControlSystem.ExtendArm().Success)
				{
					ErrorCode = "GRIPPER EXTEND TIMEOUT";
					return;
				}
				if (!base.ControlSystem.SetFinger(GripperFingerState.Closed).Success)
				{
					ErrorCode = "GRIPPER FINGER TIMEOUT";
					return;
				}
				if (!base.ControlSystem.RetractArm().Success)
				{
					ErrorCode = "GRIPPER RETRACT TIMEOUT";
					return;
				}
			}
			base.ControlSystem.SetFinger(GripperFingerState.Open);
			if (!base.ControlSystem.TrackClose().Success)
			{
				ErrorCode = "TRACK CLOSE TIMEOUT";
			}
			else if (!base.ControlSystem.RollerToPosition(RollerPosition.Position4).Success)
			{
				ErrorCode = "ROLLER POS4 TIMEOUT";
			}
		}
		finally
		{
			if ("SUCCESS" != ErrorCode)
			{
				base.Context.CreateInfoResult("MachineError", ErrorCode);
				AddError("pull-in-dvd failed.");
			}
			base.ControlSystem.TrackClose();
			base.ControlSystem.StopRoller();
			base.ControlSystem.SetFinger(GripperFingerState.Rent);
			base.ControlSystem.RetractArm();
		}
	}

	internal MSPullInDVDJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
