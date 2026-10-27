using System;
using DeviceService.ComponentModel.Requests;

namespace DeviceService.ComponentModel.Commands;

public class ReadCardCommand : BaseCommandRequest
{
	public override string SignalRCommand => "ExecuteReadCardCommand";

	public override string CommandName => "ReadCard";

	public CardReadRequest Request { get; set; }

	public ReadCardCommand(Version deviceServiceClientVersion)
		: base("ReadCard", deviceServiceClientVersion)
	{
	}
}
