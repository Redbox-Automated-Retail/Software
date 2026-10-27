namespace KioskClientService.API.Services.Analytics.Vistar.Request;

public class GetAssetAnalyticsRequest : BaseAnalyticsRequest
{
	public string AssetId { get; set; }

	public string FilePath { get; set; }

	public bool Success { get; set; }
}
