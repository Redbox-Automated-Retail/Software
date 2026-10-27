namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "reset-fraud-sensor", Operand = "RESET-FRAUD-SENSOR")]
internal sealed class ResetFraudSensorJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: true, "reset-fraud-sensor");
		base.Context.CreateInfoResult("FraudSensorNotConfigured", "The fraud sensor is not configured.");
		AddError("Failed to reset fraud sensor.");
	}

	internal ResetFraudSensorJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
