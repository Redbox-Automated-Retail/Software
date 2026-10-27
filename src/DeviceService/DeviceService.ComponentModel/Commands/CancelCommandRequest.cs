using System;

namespace DeviceService.ComponentModel.Commands;

public class CancelCommandRequest : BaseCommandRequest
{
	public override string SignalRCommand => "ExecuteCancelCommand";

	public override bool IsQueuedCommand => false;

	public Guid CommandToCancelRequestId { get; set; }

	public CancelCommandRequest(Version deviceServiceClientVersion)
		: base("Cancel", deviceServiceClientVersion)
	{
	}
}
