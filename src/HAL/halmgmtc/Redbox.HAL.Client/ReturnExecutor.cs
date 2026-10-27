namespace Redbox.HAL.Client;

public sealed class ReturnExecutor : JobExecutor
{
	protected override string JobName => "return";

	public ReturnExecutor(HardwareService service)
		: base(service)
	{
	}
}
