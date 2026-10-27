using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public class CardRemoveResponseEvent : SimpleResponseEvent
{
	public CardRemoveResponseEvent(BaseCommandRequest request)
		: base(request, "CardRemovedResponseEvent")
	{
	}
}
