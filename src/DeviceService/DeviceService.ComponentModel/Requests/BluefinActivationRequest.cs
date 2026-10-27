using Newtonsoft.Json;

namespace DeviceService.ComponentModel.Requests;

public class BluefinActivationRequest : IBluefinActivationRequest, IMessageScrub
{
	public long KioskId { get; set; }

	public string BluefinServiceUrl { get; set; }

	public string ApiKey { get; set; }

	public int Timeout { get; set; }

	public object Scrub()
	{
		return JsonConvert.SerializeObject(new
		{
			KioskId = KioskId,
			BluefinServiceUrl = BluefinServiceUrl,
			ApiKey = "*** Removed ***",
			Timeout = Timeout
		});
	}
}
