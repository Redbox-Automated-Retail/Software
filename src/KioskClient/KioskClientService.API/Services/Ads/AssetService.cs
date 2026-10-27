using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Vistar;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Ads;

public class AssetService : IAssetService
{
	private ILogger<AssetService> _logger;

	private IOptionsSnapshotKioskConfiguration _kioskConfigiuration;

	private IVistarAPIClient _vistarAPI;

	private IStoreService _storeService;

	private IHttpService _httpService;

	private IAnalyticsService _analytics;

	private bool _adsActive;

	private const int CacheDaysToKeep = 14;

	private string _assetPath;

	private string AssetPath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_assetPath))
			{
				string path = Path.Combine(_storeService.DataPath, "Ads\\Assets");
				_assetPath = Path.GetFullPath(path);
				Directory.CreateDirectory(_assetPath);
			}
			return _assetPath;
		}
	}

	public AssetService(IOptionsSnapshotKioskConfiguration kioskConfigiuration, ILogger<AssetService> logger, IVistarAPIClient vistarAPI, IStoreService storeService, IHttpService httpService, IAnalyticsService analytics)
	{
		_logger = logger;
		_kioskConfigiuration = kioskConfigiuration;
		_vistarAPI = vistarAPI;
		_storeService = storeService;
		_httpService = httpService;
		_analytics = analytics;
		_adsActive = _kioskConfigiuration.KioskClientAds.AdsActive;
	}

	public async Task<CacheResponse> Cache()
	{
		if (!_adsActive)
		{
			return new CacheResponse
			{
				StatusCode = HttpStatusCode.ServiceUnavailable
			};
		}
		GetAssetsRequest getAssetsRequest = new GetAssetsRequest(_storeService.KioskId, DisplayArea.GetAllDisplayAreas.ToVistar());
		APIResponse<VistarGetAssetsResponse> aPIResponse = await _vistarAPI.GetAssets(getAssetsRequest.KioskId, getAssetsRequest.DisplayAreas);
		CacheResponse cacheResponse = new CacheResponse();
		aPIResponse.CopyTo(cacheResponse);
		cacheResponse.ResponseTime = aPIResponse.ResponseTime;
		if (aPIResponse.IsSuccessStatusCode)
		{
			List<VistarAsset> asset = aPIResponse.Response.asset;
			if (asset == null)
			{
				_logger.LogInfoWithSource("GetAssets response did not contain any assets", "Cache");
				return cacheResponse;
			}
			ManageAssets(((IEnumerable<Asset>)asset.ToAssets()).Select((Func<Asset, BaseAsset>)((Asset a) => a)).ToList());
			cacheResponse.AssetCacheCount = asset.Count;
			return cacheResponse;
		}
		_logger.LogErrorWithSource("did not return success", "Cache");
		return cacheResponse;
	}

	public Asset GetAsset(string assetId)
	{
		_logger?.LogInformation("AssetService.GetAsset");
		Asset result = null;
		string assetFilePath = GetAssetFilePath(assetId);
		if (File.Exists(assetFilePath))
		{
			result = JsonConvert.DeserializeObject<Asset>(File.ReadAllText(assetFilePath));
		}
		return result;
	}

	public async Task<bool> ManageAssets(List<BaseAsset> list)
	{
		list.ForEach(async delegate(BaseAsset a)
		{
			await DownloadAsset(a);
		});
		PurgeOldAssets(list);
		return true;
	}

	public void PurgeOldAssets(List<BaseAsset> currentAssets)
	{
		_logger?.LogInfoWithSource("Checking for old assets to purge.", "PurgeOldAssets");
		DirectoryInfo directoryInfo = new DirectoryInfo(AssetPath);
		IEnumerable<FileInfo> enumerable = from f in directoryInfo.GetFiles("*.json")
			where f.LastWriteTime < DateTime.Now.Date.AddDays(-14.0)
			select f;
		foreach (FileInfo item in enumerable)
		{
			string assetId = Asset.GetAssetIdFromFileName(item.Name);
			if (!currentAssets.Any((BaseAsset a) => a.AssetId == assetId))
			{
				_logger?.LogInfoWithSource($"Asset id {assetId} is {(DateTime.Now.Date - item.LastWriteTime.Date).Days} days old.  Deleting the asset files.", "PurgeOldAssets");
				foreach (FileInfo item2 in directoryInfo.EnumerateFiles(assetId + ".*"))
				{
					try
					{
						item2.Delete();
					}
					catch (Exception ex)
					{
						_logger?.LogErrorWithSource("Unable delete file " + item2.Name + " : " + ex.Message, "PurgeOldAssets");
					}
				}
			}
			else
			{
				_logger?.LogInfoWithSource($"Asset id {assetId} is {(DateTime.Now.Date - item.LastWriteTime.Date).Days} days old.  In current, not deleting.", "PurgeOldAssets");
			}
		}
	}

	public async Task<bool> DownloadAsset(BaseAsset asset)
	{
		_logger?.LogInfoWithSource("downloading " + asset.AssetUrl, "DownloadAsset");
		string assetFilePath = GetAssetFilePath(asset);
		string assetCreativeFilePath = GetAssetCreativeFilePath(asset);
		if (File.Exists(assetFilePath) && File.Exists(assetCreativeFilePath))
		{
			_logger?.LogInfoWithSource("asset already exists, skipping download", "DownloadAsset");
			return true;
		}
		APIResponse<byte[]> response = await _httpService.DownloadFile(asset.AssetUrl);
		if (!response.IsSuccessStatusCode || response.ResponseLength == 0)
		{
			_logger?.LogInfoWithSource("download failed", "DownloadAsset");
			return false;
		}
		_analytics.DownloadAsset(response);
		string contents = JsonConvert.SerializeObject(asset, Formatting.Indented, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		});
		await File.WriteAllTextAsync(assetFilePath, contents);
		await File.WriteAllBytesAsync(assetCreativeFilePath, response.Response);
		return true;
	}

	private string GetAssetFilePath(BaseAsset asset)
	{
		return Path.Combine(AssetPath, asset.GetAssetFileName());
	}

	private string GetAssetFilePath(string assetId)
	{
		return Path.Combine(AssetPath, Asset.GetAssetFileName(assetId));
	}

	public string GetAssetCreativeFilePath(BaseAsset asset)
	{
		return Path.Combine(AssetPath, asset.GetAssetCreativeFileName());
	}
}
