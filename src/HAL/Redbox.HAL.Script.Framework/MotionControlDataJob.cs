using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "motion-control-get-data")]
internal sealed class MotionControlDataJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		IControllerPosition controllerPosition = service.ReadPositions();
		if (!controllerPosition.ReadOk)
		{
			base.Context.CreateInfoResult("PositionReadError", ErrorCodes.ArcusNotResponsive.ToString());
		}
		else
		{
			if (controllerPosition.XCoordinate.HasValue)
			{
				base.Context.CreateInfoResult("PositionX", controllerPosition.XCoordinate.Value.ToString());
			}
			if (controllerPosition.YCoordinate.HasValue)
			{
				base.Context.CreateInfoResult("PositionY", controllerPosition.YCoordinate.Value.ToString());
			}
		}
		IMotionControlLimitResponse motionControlLimitResponse = service.ReadLimits();
		if (motionControlLimitResponse.ReadOk)
		{
			IMotionControlLimit[] limits = motionControlLimitResponse.Limits;
			foreach (IMotionControlLimit motionControlLimit in limits)
			{
				base.Context.CreateInfoResult(motionControlLimit.Limit.ToString(), motionControlLimit.Blocked ? "BLOCKED" : "CLEAR");
			}
		}
		else
		{
			base.Context.CreateInfoResult("LimitReadError", ErrorCodes.ArcusNotResponsive.ToString());
		}
		if (service.AtVendDoor)
		{
			base.Context.CreateInfoResult("CurrentLocation", "VENDDOOR");
		}
		else if (service.CurrentLocation == null)
		{
			base.Context.CreateInfoResult("CurrentLocation", "UNKNOWN");
		}
		else
		{
			base.Context.CreateInfoResult("CurrentLocation", $"Deck = {service.CurrentLocation.Deck}, Slot = {service.CurrentLocation.Slot}");
		}
	}

	internal MotionControlDataJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
