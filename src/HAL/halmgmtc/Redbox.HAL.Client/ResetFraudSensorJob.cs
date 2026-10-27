namespace Redbox.HAL.Client;

public sealed class ResetFraudSensorJob : JobExecutor
{
	protected override string JobName => "reset-fraud-sensor";

	public ResetFraudSensorJob(HardwareService service)
		: base(service)
	{
	}
}
