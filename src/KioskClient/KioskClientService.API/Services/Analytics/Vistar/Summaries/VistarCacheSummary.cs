namespace KioskClientService.API.Services.Analytics.Vistar.Summaries;

public class VistarCacheSummary : HttpSummary
{
	public double? MedianAssetsCached { get; set; }

	public int MaxAssetsCached { get; set; }

	public int MinAssetsCached { get; set; }
}
