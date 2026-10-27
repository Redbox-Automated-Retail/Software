namespace Redbox.HAL.Client.Executors;

public sealed class PushInDvdJob : JobExecutor
{
	protected override string JobName => "push-in-dvd";

	public PushInDvdJob(HardwareService service)
		: base(service)
	{
	}
}
