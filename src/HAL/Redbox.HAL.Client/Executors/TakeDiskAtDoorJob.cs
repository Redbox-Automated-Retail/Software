namespace Redbox.HAL.Client.Executors;

public sealed class TakeDiskAtDoorJob : JobExecutor
{
	protected override string JobName => "ms-take-disk-at-door";

	public TakeDiskAtDoorJob(HardwareService service)
		: base(service)
	{
	}
}
