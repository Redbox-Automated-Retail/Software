using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Analytics.Vistar.Summaries;

namespace KioskClientService.API.Services.Analytics.Vistar;

public interface IAdServerAnalyticsClient
{
	Task<APIResponse<AnalyticsResponse>> PostGetAdAnalytics(VistarGetAdSummary request);

	Task<APIResponse<AnalyticsResponse>> PostProofOfPlayAnalytics(VistarProofOfPlaySummary request);

	Task<APIResponse<AnalyticsResponse>> PostExpireAnalytics(VistarExpireSummary request);

	Task<APIResponse<AnalyticsResponse>> PostCacheAnalytics(VistarCacheSummary request);

	Task<APIResponse<AnalyticsResponse>> PostGetAssetAnalytics(VistarGetAssetSummary request);

	Task<APIResponse<AnalyticsResponse>> PostDownloadAssetAnalytics(VistarDownloadAssetSummary request);

	Task<APIResponse<AnalyticsResponse>> PostGetAdTimeoutEventAnalytics(GetAdTimeoutEventSummary request);
}
