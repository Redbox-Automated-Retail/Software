using System.Collections.Generic;

namespace KioskClientService.API.Vistar;

public class VistarGetAdRequest
{
	public string device_id { get; set; }

	public bool direct_connection { get; set; }

	public List<VistarDeviceAttribute> device_attribute { get; set; }

	public List<VistarDisplayArea> display_area { get; set; }

	public long display_time { get; set; }

	public string venue_id { get; set; }

	public string network_id { get; set; }

	public string api_key { get; set; }

	public float? latitude { get; set; }

	public float? longitude { get; set; }
}
