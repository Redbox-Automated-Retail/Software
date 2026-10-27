using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class DeviceServiceShutDownResponseEvent : BaseResponseEvent
{
	public DeviceServiceShutDownResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "DeviceServiceShutDownResponseEvent";
	}
}
