using System;

namespace DeviceService.ComponentModel.Commands;

public class IsConnectedCommandRequest : BaseCommandRequest
{
	public override bool IsQueuedCommand => false;

	public IsConnectedCommandRequest(Version deviceServiceClientVersion)
		: base("IsConnected", deviceServiceClientVersion)
	{
	}
}
