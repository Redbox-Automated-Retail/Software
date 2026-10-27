namespace Redbox.HAL.Client.Executors;

public sealed class TestAndResetCCRExecutor : JobExecutor
{
	protected override string JobName => "test-and-reset-ccr";

	public TestAndResetCCRExecutor(HardwareService service)
		: base(service)
	{
	}
}
