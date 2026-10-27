namespace Redbox.HAL.Client;

public sealed class ResetMotionControlExecutor : JobExecutor
{
	protected override string JobName => "reset-motioncontrol";

	public ResetMotionControlExecutor(HardwareService service)
		: base(service)
	{
	}
}
