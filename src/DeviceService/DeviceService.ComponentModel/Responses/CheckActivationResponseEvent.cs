using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class CheckActivationResponseEvent : BaseResponseEvent
{
	public CheckActivationResponseEvent(BaseCommandRequest baseCommandRequest)
		: base(baseCommandRequest)
	{
		base.EventName = "CheckActivationResponseEvent";
	}
}
