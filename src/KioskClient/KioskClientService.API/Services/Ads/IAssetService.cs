using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Ads;

public interface IAssetService
{
	Task<CacheResponse> Cache();

	Asset GetAsset(string assetId);

	string GetAssetCreativeFilePath(BaseAsset asset);

	Task<bool> DownloadAsset(BaseAsset asset);

	void PurgeOldAssets(List<BaseAsset> currentAssets);

	Task<bool> ManageAssets(List<BaseAsset> list);
}
