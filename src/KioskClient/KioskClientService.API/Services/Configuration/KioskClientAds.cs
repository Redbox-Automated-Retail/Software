namespace KioskClientService.API.Services.Configuration;

public class KioskClientAds : BaseCategorySetting
{
	public bool AdsActive { get; set; } = true;

	public bool AdAnalyticsActive { get; set; }

	public string VistarUrl { get; set; }

	public int VistarTimeout { get; set; } = 5000;

	[MaskLogValue(VisibleChars = 4)]
	public string VistarNetworkId { get; set; }

	[MaskLogValue(VisibleChars = 4)]
	public string VistarAPIKey { get; set; }

	public bool AdServerEnabled { get; set; } = true;

	public string AdServerUrl { get; set; } = "http://localhost:5005/api";

	public int AdServerTimeout { get; set; } = 20000;

	[MaskLogValue(VisibleChars = 4)]
	public string AdServerAPIKey { get; set; }

	public string AdServerAuthHeader { get; set; } = "x-api-key";
}
