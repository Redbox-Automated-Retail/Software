using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class SupportsEMVResponseEvent : BaseResponseEvent
{
	public bool SuportsEMV { get; set; }

	public SupportsEMVResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "SupportsEMVResponseEvent";
	}
}
