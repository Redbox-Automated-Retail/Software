namespace Redbox.HAL.Client.Executors;

public sealed class BoardTestJob : JobExecutor
{
	protected override string JobName => "test-boards-job";

	public BoardTestJob(HardwareService service)
		: base(service)
	{
	}
}
