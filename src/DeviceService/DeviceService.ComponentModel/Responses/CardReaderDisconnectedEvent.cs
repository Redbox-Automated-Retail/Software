namespace DeviceService.ComponentModel.Responses;

public class CardReaderDisconnectedEvent : SimpleEvent
{
	public CardReaderDisconnectedEvent()
	{
		base.EventName = "CardReaderDisconnectedEvent";
	}
}
