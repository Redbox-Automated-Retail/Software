using System;
using DeviceService.ComponentModel.Requests;

namespace DeviceService.ComponentModel.Commands;

public class ReadCardCommand : BaseCommandRequest
{
	public override string SignalRCommand => "ExecuteReadCardCommand";

	public override string CommandName => "ReadCard";

	// Set by the sender's object initializer (DeviceServiceClient's read card commands).
	public CardReadRequest Request { get; set; } = null!;

	public ReadCardCommand(Version deviceServiceClientVersion)
		: base("ReadCard", deviceServiceClientVersion)
	{
	}
}
