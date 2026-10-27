using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class SimpleResponseEvent : BaseResponseEvent
{
	public string Data { get; set; }

	public SimpleResponseEvent(BaseCommandRequest request, string eventName, string data = null, bool success = true)
		: base(request)
	{
		base.EventName = eventName;
		Data = data;
		base.Success = success;
	}
}
