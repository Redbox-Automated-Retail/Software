using System;
using DeviceService.ComponentModel.Requests;
using Newtonsoft.Json;

namespace DeviceService.ComponentModel.Commands;

public class CheckDeviceStatusCommand : BaseCommandRequest
{
	public override string SignalRCommand => "ExecuteCheckDeviceStatusCommand";

	public override string CommandName => "CheckDeviceStatus";

	public DeviceStatusRequest Request { get; set; }

	public CheckDeviceStatusCommand(Version deviceServiceClientVersion)
		: base("CheckDeviceStatus", deviceServiceClientVersion)
	{
	}

	public override object Scrub()
	{
		CheckDeviceStatusCommand checkDeviceStatusCommand = JsonConvert.DeserializeObject<CheckDeviceStatusCommand>(JsonConvert.SerializeObject(this));
		checkDeviceStatusCommand.Request = new DeviceStatusRequest
		{
			ApiKey = "*** Removed ***",
			ApiUrl = Request.ApiUrl,
			KioskId = Request.KioskId,
			Status = Request.Status
		};
		return checkDeviceStatusCommand;
	}
}
