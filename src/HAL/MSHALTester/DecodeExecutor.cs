using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal class DecodeExecutor : JobExecutor
{
	private readonly string File;

	protected override string JobName => "decode-image";

	internal ScanResult ScanResult { get; private set; }

	protected override void SetupJob()
	{
		base.Job.Push(File);
	}

	protected override void OnJobCompleted()
	{
		ScanResult = ScanResult.From(base.Results);
	}

	internal DecodeExecutor(HardwareService service, string file)
		: base(service)
	{
		File = file;
	}
}
