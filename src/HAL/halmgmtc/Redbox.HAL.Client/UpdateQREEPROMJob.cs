namespace Redbox.HAL.Client;

public sealed class UpdateQREEPROMJob : JobExecutor
{
	protected override string JobName => "update-qr-eeprom";

	protected override string Label => "Utilities scheduled update";

	public UpdateQREEPROMJob(HardwareService service)
		: base(service)
	{
	}
}
