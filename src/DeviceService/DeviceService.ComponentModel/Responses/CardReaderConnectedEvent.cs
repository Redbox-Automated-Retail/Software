namespace DeviceService.ComponentModel.Responses;

public class CardReaderConnectedEvent : SimpleEvent
{
	public CardReaderConnectedEvent()
	{
		base.EventName = "CardReaderConnectedEvent";
	}
}
