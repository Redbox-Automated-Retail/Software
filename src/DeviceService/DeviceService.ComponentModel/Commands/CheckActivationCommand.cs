using System;
using DeviceService.ComponentModel.Requests;
using Newtonsoft.Json;

namespace DeviceService.ComponentModel.Commands;

public class CheckActivationCommand : BaseCommandRequest
{
	public BluefinActivationRequest Request { get; set; }

	public override string SignalRCommand => "ExecuteCheckActivationCommand";

	public CheckActivationCommand(Version deviceServiceClientVersion)
		: base("CheckActivation", deviceServiceClientVersion)
	{
	}

	public override object Scrub()
	{
		return JsonConvert.SerializeObject(new
		{
			Request = Request.Scrub(),
			SignalRCommand = SignalRCommand,
			RequestId = base.RequestId,
			CommandName = CommandName,
			CreatedDate = base.CreatedDate,
			CommandTimeout = base.CommandTimeout,
			DeviceServiceClientVersion = base.DeviceServiceClientVersion
		});
	}
}
