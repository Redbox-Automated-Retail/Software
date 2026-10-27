namespace Redbox.HAL.Client.Executors;

public sealed class ResetControlSystemJob : JobExecutor
{
	protected override string JobName => "reset-controlsystem-job";

	public ResetControlSystemJob(HardwareService service)
		: base(service)
	{
	}
}
