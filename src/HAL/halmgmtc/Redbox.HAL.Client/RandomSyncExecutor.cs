namespace Redbox.HAL.Client;

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

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(VendTime);
		job.Push(VendFrequency);
	}
}
