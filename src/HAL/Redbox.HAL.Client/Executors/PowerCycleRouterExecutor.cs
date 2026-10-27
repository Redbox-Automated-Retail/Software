namespace Redbox.HAL.Client.Executors;

public sealed class PowerCycleRouterExecutor : JobExecutor
{
	protected override string JobName => "power-cycle-router";

	public PowerCycleRouterExecutor(HardwareService service)
		: base(service)
	{
	}
}
