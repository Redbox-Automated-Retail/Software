using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-motioncontrol", Operand = "RESET-MOTIONCONTROL")]
internal sealed class ResetArcusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ErrorCodes errorCodes = base.MotionService.Reset(quick: false);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("ResetFailure", $"RESET of the controller failed with error {errorCodes}.");
			AddError("Reset failure.");
		}
		else
		{
			base.Context.CreateInfoResult("ResetSuccess", "RESET OK.");
		}
	}

	internal ResetArcusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
