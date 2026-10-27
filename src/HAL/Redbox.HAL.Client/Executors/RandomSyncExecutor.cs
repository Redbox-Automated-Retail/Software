namespace Redbox.HAL.Client.Executors;

public sealed class RandomSyncExecutor : JobExecutor
{
	private readonly int VendTime;

	private readonly int VendFrequency;

	protected override string JobName => "random-sync";

	public RandomSyncExecutor(HardwareService service, int vendTime, int frequency)
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
