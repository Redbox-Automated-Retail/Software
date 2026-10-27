namespace DeviceService.ComponentModel.Responses;

public class DeviceServiceShutDownStartingEvent : SimpleEvent
{
	public ShutDownReason ShutDownReason { get; set; }

	public DeviceServiceShutDownStartingEvent()
	{
		base.EventName = "DeviceServiceShutDownStartingEvent";
	}
}
