namespace Redbox.HAL.Client.Executors;

public sealed class PutInEmptySlotResult : JobExecutor
{
	protected override string JobName => "file-disk-in-picker";

	public PutInEmptySlotResult(HardwareService service)
		: base(service)
	{
	}
}
