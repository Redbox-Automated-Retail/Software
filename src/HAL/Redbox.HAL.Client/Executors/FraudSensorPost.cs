namespace Redbox.HAL.Client.Executors;

public sealed class FraudSensorPost : JobExecutor
{
	protected override string JobName => "fraud-sensor-post-test";

	public FraudSensorPost(HardwareService service)
		: base(service)
	{
	}
}
