using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class UnencryptedCardReadResponseEvent : BaseResponseEvent, ICardReadResponseEvent
{
	public UnencryptedCardReadModel Data { get; set; }

	public UnencryptedCardReadResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "UnencryptedCardReadResponseEvent";
	}

	public Base87CardReadModel GetBase87CardReadModel()
	{
		return Data;
	}
}
