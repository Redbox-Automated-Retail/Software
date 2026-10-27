namespace DeviceService.ComponentModel.Responses;

public class DeviceTamperedEvent : SimpleEvent
{
	public DeviceTamperedEvent()
	{
		base.EventName = "DeviceTamperedEvent";
	}
}
