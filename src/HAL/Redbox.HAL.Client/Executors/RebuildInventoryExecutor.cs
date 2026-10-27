namespace Redbox.HAL.Client.Executors;

public sealed class RebuildInventoryExecutor : JobExecutor
{
	protected override string JobName => "rebuild-inventory-database";

	public RebuildInventoryExecutor(HardwareService service)
		: base(service)
	{
	}
}
