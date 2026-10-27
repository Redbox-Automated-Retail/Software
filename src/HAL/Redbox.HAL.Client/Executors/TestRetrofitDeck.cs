namespace Redbox.HAL.Client.Executors;

public sealed class TestRetrofitDeck : JobExecutor
{
	protected override string JobName => "test-retrofit-deck";

	public TestRetrofitDeck(HardwareService service)
		: base(service)
	{
	}
}
