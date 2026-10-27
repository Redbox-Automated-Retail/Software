using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class GetUnitHealthResponseEvent : BaseResponseEvent
{
	public UnitHealthModel UnitHealthModel { get; set; }

	public GetUnitHealthResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "GetUnitHealthResponseEvent";
	}
}
