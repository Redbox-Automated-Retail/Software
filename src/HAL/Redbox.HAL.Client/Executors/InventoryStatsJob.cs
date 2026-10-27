namespace Redbox.HAL.Client.Executors;

public sealed class InventoryStatsJob : JobExecutor
{
	protected override string JobName => "get-inventory-stats";

	public InventoryStatsJob(HardwareService service)
		: base(service)
	{
	}
}
