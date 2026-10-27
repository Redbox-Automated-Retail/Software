namespace Redbox.HAL.Client.Executors;

public sealed class OneDiskRandomSyncExecutor : JobExecutor
{
	private readonly int VendTime;

	private readonly int VendFrequency;

	protected override string JobName => "one-disk-random-sync";

	public OneDiskRandomSyncExecutor(HardwareService service, int vendTime, int frequency)
		: base(service)
	{
		VendTime = vendTime;
		VendFrequency = frequency;
	}

	protected override void SetupJob()
	{
		base.Job.Push(VendTime);
		base.Job.Push(VendFrequency);
	}
}
