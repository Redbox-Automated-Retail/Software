namespace Redbox.HAL.Client.Executors;

public sealed class ReadFraudDiskExecutor : JobExecutor
{
	protected override string JobName => "read-fraud-disc";

	public ReadFraudDiskExecutor(HardwareService service)
		: base(service)
	{
	}
}
