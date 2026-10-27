namespace DeviceService.ComponentModel.Requests;

public class DeviceStatusRequest
{
	public long KioskId { get; set; }

	public string ApiKey { get; set; }

	public string ApiUrl { get; set; }

	public DeviceStatus Status { get; set; }
}
