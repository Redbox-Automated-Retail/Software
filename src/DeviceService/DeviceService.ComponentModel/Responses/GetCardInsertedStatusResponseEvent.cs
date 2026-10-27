using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class GetCardInsertedStatusResponseEvent : BaseResponseEvent
{
	public InsertedStatus CardInsertedStatus { get; set; }

	public GetCardInsertedStatusResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "GetCardInsertedStatusResponseEvent";
	}
}
