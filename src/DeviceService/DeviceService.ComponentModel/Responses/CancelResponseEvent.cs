using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class CancelResponseEvent : BaseResponseEvent
{
	public CancelResponseEvent(BaseCommandRequest request)
		: base(request)
	{
	}
}
