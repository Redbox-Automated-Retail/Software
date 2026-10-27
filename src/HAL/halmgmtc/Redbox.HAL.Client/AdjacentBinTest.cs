namespace Redbox.HAL.Client;

public sealed class AdjacentBinTest : JobExecutor
{
	protected override string JobName => "adjacent-bin-test";

	public AdjacentBinTest(HardwareService service)
		: base(service)
	{
	}
}
