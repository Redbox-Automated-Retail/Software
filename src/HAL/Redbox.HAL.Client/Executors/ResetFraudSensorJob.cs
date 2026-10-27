namespace Redbox.HAL.Client.Executors;

public sealed class ResetFraudSensorJob : JobExecutor
{
	protected override string JobName => "reset-fraud-sensor";

	public ResetFraudSensorJob(HardwareService service)
		: base(service)
	{
	}
}
