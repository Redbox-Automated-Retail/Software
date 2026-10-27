namespace Redbox.HAL.Client;

public sealed class PutDiskInPickerInBinJob : JobExecutor
{
	protected override string JobName => "put-disk-in-picker-in-bin";

	public PutDiskInPickerInBinJob(HardwareService service)
		: base(service)
	{
	}
}
