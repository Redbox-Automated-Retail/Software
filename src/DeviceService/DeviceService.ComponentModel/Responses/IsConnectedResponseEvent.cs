using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class IsConnectedResponseEvent : BaseResponseEvent
{
	public bool IsConnected { get; set; }

	public IsConnectedResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "IsConnectedResponseEvent";
	}
}
