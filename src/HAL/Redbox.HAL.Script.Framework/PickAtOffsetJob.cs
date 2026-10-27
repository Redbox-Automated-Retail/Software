using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "pick-at-offset")]
internal sealed class PickAtOffsetJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int deck = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		Axis axis = base.Context.PopTop<Axis>();
		int value = base.Context.PopTop<int>();
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = service2.Get(deck, slot);
		OffsetMoveData data = ((axis == Axis.X) ? new OffsetMoveData(value, 0) : new OffsetMoveData(0, value));
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(location, MoveMode.None, service.NilLog, ref data);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("MoveError", $"MOVE failed with status {errorCodes.ToString().ToUpper()}");
			AddError("Hardware Error");
			return;
		}
		base.ControlSystem.TrackOpen();
		IControlResponse controlResponse = null;
		for (int i = 0; i < 2; i++)
		{
			controlResponse = base.ControlSystem.SetFinger(GripperFingerState.Rent);
			if (!controlResponse.Success)
			{
				FormatPickError("GripperRentFailure", controlResponse);
				return;
			}
			controlResponse = base.ControlSystem.ExtendArm();
			if (!controlResponse.Success)
			{
				if (controlResponse.TimedOut)
				{
					base.ControlSystem.RetractArm();
				}
				FormatPickError("GripperExtendFailure", controlResponse);
				return;
			}
			controlResponse = base.ControlSystem.SetFinger(GripperFingerState.Closed);
			if (!controlResponse.Success)
			{
				FormatPickError("GripperCloseFailure", controlResponse);
				return;
			}
			controlResponse = base.ControlSystem.RetractArm();
			if (!controlResponse.Success)
			{
				FormatPickError("GripperRetractFailure", controlResponse);
				return;
			}
		}
		controlResponse = base.ControlSystem.SetFinger(GripperFingerState.Rent);
		if (!controlResponse.Success)
		{
			FormatPickError("GripperRentFailure", controlResponse);
			return;
		}
		base.ControlSystem.StartRollerOut();
		Thread.Sleep(150);
		controlResponse = base.ControlSystem.TrackClose();
		if (!controlResponse.Success)
		{
			FormatPickError("TrackCloseFailure", controlResponse);
			return;
		}
		controlResponse = base.ControlSystem.RollerToPosition(RollerPosition.Position4);
		if (!controlResponse.Success)
		{
			FormatPickError("RollerToPos4Failure", controlResponse);
			return;
		}
		base.Context.CreateInfoResult("PickSuccess", "Successfully picked the disk.");
		service2.Reset(location);
	}

	internal PickAtOffsetJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void FormatPickError(string s, IControlResponse response)
	{
		base.Context.CreateInfoResult("PickError", $"GET failed: {s} ({response})");
		AddError("Hardware Error");
	}
}
