namespace Redbox.HAL.Client;

public sealed class InitJob : JobExecutor
{
	protected override string JobName => "init";

	protected override string Label => "MS Tester Init";

	public InitJob(HardwareService service)
		: base(service)
	{
	}
}
