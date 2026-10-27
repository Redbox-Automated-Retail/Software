namespace Redbox.HAL.Client;

public sealed class OneDiskQuickTest : JobExecutor
{
	protected override string JobName => "one-disk-quick-deck-test";

	public OneDiskQuickTest(HardwareService service)
		: base(service)
	{
	}
}
