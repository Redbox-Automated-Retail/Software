using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class EncryptedCardReadResponseEvent : BaseResponseEvent, ICardReadResponseEvent
{
	public EncryptedCardReadModel Data { get; set; }

	public EncryptedCardReadResponseEvent(BaseCommandRequest request)
		: base(request)
	{
		base.EventName = "EncryptedCardReadResponseEvent";
	}

	public Base87CardReadModel GetBase87CardReadModel()
	{
		return Data;
	}
}
