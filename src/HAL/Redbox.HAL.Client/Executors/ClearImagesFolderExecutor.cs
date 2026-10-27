namespace Redbox.HAL.Client.Executors;

public sealed class ClearImagesFolderExecutor : JobExecutor
{
	protected override string JobName => "clear-images-folder";

	public ClearImagesFolderExecutor(HardwareService service)
		: base(service)
	{
	}
}
