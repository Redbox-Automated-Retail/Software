using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class ReportAuthorizationResultResponseEvent : BaseResponseEvent
{
	public ReportAuthorizationResultResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "ReportAuthorizationResultResponseEvent";
	}
}
