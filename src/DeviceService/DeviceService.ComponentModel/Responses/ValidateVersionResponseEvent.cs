using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class ValidateVersionResponseEvent : BaseResponseEvent
{
	public ValidateVersionModel ValidateVersionModel { get; set; }

	public ValidateVersionResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "ValidateVersionReponseEvent";
	}
}
