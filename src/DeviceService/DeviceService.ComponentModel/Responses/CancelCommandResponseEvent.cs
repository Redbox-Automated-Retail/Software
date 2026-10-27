using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class CancelCommandResponseEvent : BaseResponseEvent
{
	public CancelCommandResponseEvent(BaseCommandRequest baseCommandRequest)
		: base(baseCommandRequest)
	{
		base.EventName = "CancelCommandResponseEvent";
	}
}
