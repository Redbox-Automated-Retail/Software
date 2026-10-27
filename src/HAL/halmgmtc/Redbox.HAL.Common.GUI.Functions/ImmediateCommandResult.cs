using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public class ImmediateCommandResult
{
	public string Message { get; internal set; }

	public HardwareCommandResult CommandResult { get; internal set; }
}
