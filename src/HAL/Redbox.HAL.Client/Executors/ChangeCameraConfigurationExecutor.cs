using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class ChangeCameraConfigurationExecutor : JobExecutor
{
	private readonly ScannerServices ChangeTo;

	protected override string JobName => "change-camera-configuration";

	public ChangeCameraConfigurationExecutor(HardwareService service, ScannerServices newService)
		: base(service)
	{
		ChangeTo = newService;
	}

	protected override void SetupJob()
	{
		base.Job.Push(ChangeTo.ToString());
	}
}
