using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "hardware-status")]
internal sealed class HardwareStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		int num = 0;
		if (ControllerConfiguration.Instance.FixArcusDuringStatusJob && !base.MotionService.TestAndReset())
		{
			base.Context.CreateInfoResult("MotionControllerCommunicationError", "Could not talk with the motion controller.");
			num++;
		}
		if (ControllerConfiguration.Instance.EnableDoorCheckInStatus && service.Query() != DoorSensorResult.Ok)
		{
			base.Context.CreateInfoResult("MotionControllerCommunicationError", "The door sensors are not in a safe mode.");
			num++;
		}
		if (ServiceLocator.Instance.GetService<IControllerService>().ClearGripper() != ErrorCodes.Success)
		{
			num++;
			base.Context.CreateInfoResult("GripperObstructed", "The gripper is obstructed.");
		}
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success || pickerSensorReadResult.IsFull)
		{
			num++;
			base.Context.CreateInfoResult("GripperObstructed", "The picker is full.");
		}
		if (VendDoorState.Closed != base.ControlSystem.VendDoorState && !base.ControlSystem.VendDoorClose().Success)
		{
			num++;
			base.Context.CreateInfoResult("VendDoorOpen", "Unable to close the vend door.");
		}
		if (!TestInventoryStore())
		{
			num++;
		}
		if (num == 0)
		{
			base.Context.CreateInfoResult("HardwareStatusOk", "The three tests passed!");
		}
		else
		{
			base.Context.CreateInfoResult("HardwareStatusInError", "One or more ops failed.");
		}
	}

	internal HardwareStatusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
