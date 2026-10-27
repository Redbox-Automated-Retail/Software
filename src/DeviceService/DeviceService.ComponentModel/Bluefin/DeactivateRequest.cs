namespace DeviceService.ComponentModel.Bluefin;

public class DeactivateRequest
{
	public long KioskId { get; set; }

	public string ReaderSerialNumber { get; set; }
}
