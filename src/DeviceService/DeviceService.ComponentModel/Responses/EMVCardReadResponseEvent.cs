using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class EMVCardReadResponseEvent : BaseResponseEvent, ICardReadResponseEvent
{
	public EMVCardReadModel Data { get; set; }

	public EMVCardReadResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "EMVCardReadResponseEvent";
	}

	public Base87CardReadModel GetBase87CardReadModel()
	{
		return Data;
	}
}
