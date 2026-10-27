namespace DeviceService.ComponentModel.Responses;

public class CardReaderStateEvent : SimpleEvent
{
	public CardReaderState CardReaderState { get; set; }

	public CardReaderStateEvent()
	{
		base.EventName = "CardReaderStateEvent";
	}
}
