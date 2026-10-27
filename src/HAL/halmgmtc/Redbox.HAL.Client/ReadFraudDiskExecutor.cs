namespace Redbox.HAL.Client;

public sealed class ReadFraudDiskExecutor : JobExecutor
{
	protected override string JobName => "read-fraud-disc";

	public ReadFraudDiskExecutor(HardwareService service)
		: base(service)
	{
	}
}
