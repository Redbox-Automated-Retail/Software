namespace Redbox.HAL.Client.Services;

public class ImmediateCommandResult
{
	public string Message { get; internal set; }

	public HardwareCommandResult CommandResult { get; internal set; }
}
