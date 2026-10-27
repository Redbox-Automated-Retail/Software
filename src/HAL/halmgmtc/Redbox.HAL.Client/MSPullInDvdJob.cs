namespace Redbox.HAL.Client;

public sealed class MSPullInDvdJob : JobExecutor
{
	protected override string JobName => "ms-pull-in-dvd";

	public MSPullInDvdJob(HardwareService service)
		: base(service)
	{
	}
}
