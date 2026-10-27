namespace Redbox.HAL.Client;

public sealed class VendDiskInPickerJob : JobExecutor
{
	protected override string JobName => "ms-vend-disk-in-picker";

	public VendDiskInPickerJob(HardwareService service)
		: base(service)
	{
	}
}
