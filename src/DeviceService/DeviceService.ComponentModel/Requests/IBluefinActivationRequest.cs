namespace DeviceService.ComponentModel.Requests;

public interface IBluefinActivationRequest
{
	long KioskId { get; set; }

	string BluefinServiceUrl { get; set; }

	string ApiKey { get; set; }

	int Timeout { get; set; }
}
