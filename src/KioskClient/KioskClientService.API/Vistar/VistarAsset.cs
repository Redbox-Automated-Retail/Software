namespace KioskClientService.API.Vistar;

public class VistarAsset
{
	public string asset_id { get; set; }

	public string creative_id { get; set; }

	public string order_id { get; set; }

	public string campaign_id { get; set; }

	public string asset_url { get; set; }

	public int width { get; set; }

	public int height { get; set; }

	public string mime_type { get; set; }

	public int length_in_seconds { get; set; }

	public int length_in_milliseconds { get; set; }

	public string creative_category { get; set; }

	public string advertiser { get; set; }

	public string creative_name { get; set; }
}
