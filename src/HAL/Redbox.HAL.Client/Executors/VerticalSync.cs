namespace Redbox.HAL.Client.Executors;

public sealed class VerticalSync : JobExecutor
{
	private readonly int Slot;

	protected override string JobName => "vertical-sync";

	public VerticalSync(HardwareService service, int slot)
		: base(service)
	{
		Slot = slot;
	}

	protected override void SetupJob()
	{
		base.Job.Push(Slot);
	}
}
