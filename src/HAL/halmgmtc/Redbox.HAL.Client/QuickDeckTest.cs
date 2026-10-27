namespace Redbox.HAL.Client;

public sealed class QuickDeckTest : JobExecutor
{
	protected override string JobName => "quick-deck-test";

	public QuickDeckTest(HardwareService service)
		: base(service)
	{
	}
}
