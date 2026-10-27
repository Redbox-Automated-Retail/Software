namespace DeviceService.ComponentModel.Responses;

public class DeviceServiceCanShutDownEvent : SimpleEvent
{
	public DeviceServiceCanShutDownEvent()
	{
		base.EventName = "DeviceServiceCanShutDownEvent";
	}
}
