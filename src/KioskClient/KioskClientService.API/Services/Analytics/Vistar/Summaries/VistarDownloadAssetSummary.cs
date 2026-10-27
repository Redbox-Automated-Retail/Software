namespace KioskClientService.API.Services.Analytics.Vistar.Summaries;

public class VistarDownloadAssetSummary : HttpSummary
{
	public int TotalBytesDownloaded { get; set; }

	public double? MedianBytesDownloaded { get; set; }

	public int MaxBytesDownloaded { get; set; }
}
