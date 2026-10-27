using System.Collections.Generic;

namespace KioskClientService.API.Vistar;

public class VistarAdvertisement
{
	public string id { get; set; }

	public string proof_of_play_url { get; set; }

	public string expiration_url { get; set; }

	public long lease_expiry { get; set; }

	public string display_area_id { get; set; }

	public string asset_id { get; set; }

	public string asset_url { get; set; }

	public int width { get; set; }

	public int height { get; set; }

	public string mime_type { get; set; }

	public int? length_in_seconds { get; set; }

	public int? length_in_milliseconds { get; set; }

	public long display_time { get; set; }

	public string creative_id { get; set; }

	public string advertiser { get; set; }

	public string deal_id { get; set; }

	public string order_id { get; set; }

	public string creative_category { get; set; }

	public List<VistarCreativeCompanionKVP> creative_companion { get; set; }
}
