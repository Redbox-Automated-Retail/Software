namespace Redbox.HAL.Client.Executors;

public sealed class ReturnUnknownExecutor : JobExecutor
{
	protected override string JobName => "return-unknown";

	public ReturnUnknownExecutor(HardwareService service)
		: base(service)
	{
	}
}
