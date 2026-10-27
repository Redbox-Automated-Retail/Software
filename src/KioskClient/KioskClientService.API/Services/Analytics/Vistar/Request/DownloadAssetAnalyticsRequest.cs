namespace KioskClientService.API.Services.Analytics.Vistar.Request;

public class DownloadAssetAnalyticsRequest : HttpAnalyticsRequest
{
	public int BytesDownloaded { get; set; }
}
