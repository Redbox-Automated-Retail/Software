using System;

namespace DeviceService.ComponentModel.Commands;

public class ValidateVersionCommand : BaseCommandRequest
{
	public override string SignalRCommand => "ExecuteValidateVersionCommand";

	public override bool IsQueuedCommand => false;

	public ValidateVersionCommand(Version deviceServiceClientVersion)
		: base("ValidateVersion", deviceServiceClientVersion)
	{
	}
}
