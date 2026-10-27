using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "push-in-dvd", Operand = "PUSH-IN-DVD")]
internal sealed class PushInDVDJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		try
		{
			base.ControlSystem.StartRollerIn();
			service.Wait(500);
			base.ControlSystem.StopRoller();
			if (!base.ControlSystem.TrackOpen().Success)
			{
				HandleMachineError("TRACK OPEN TIMEOUT");
				return;
			}
			if (!base.ControlSystem.SetFinger(GripperFingerState.Closed).Success)
			{
				HandleMachineError("GRIPPER CLOSE TIMEOUT");
				return;
			}
			int pushTime = ControllerConfiguration.Instance.PushTime;
			base.ControlSystem.ExtendArm(pushTime);
			if (!base.ControlSystem.SetFinger(GripperFingerState.Rent).Success)
			{
				HandleMachineError("GRIPPER RENT TIMEOUT");
				return;
			}
			if (!base.ControlSystem.RetractArm().Success)
			{
				HandleMachineError("GRIPPER RETRACT TIMEOUT");
				return;
			}
			if (!base.ControlSystem.SetFinger(GripperFingerState.Closed).Success)
			{
				HandleMachineError("GRIPPER CLOSE TIMEOUT");
				return;
			}
			base.ControlSystem.ExtendArm(pushTime);
			if (!base.ControlSystem.SetFinger(GripperFingerState.Rent).Success)
			{
				HandleMachineError("GRIPPER RENT TIMEOUT");
			}
			else if (!base.ControlSystem.RetractArm().Success)
			{
				HandleMachineError("GRIPPER RETRACT TIMEOUT");
			}
		}
		finally
		{
			base.ControlSystem.TrackClose();
			base.ControlSystem.RetractArm();
		}
	}

	internal PushInDVDJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void HandleMachineError(string error)
	{
		base.Context.SetSymbolValue("ERROR-STATE", error);
		base.Context.CreateInfoResult("MachineError", "There was an error pushing the DVD in.");
	}
}
