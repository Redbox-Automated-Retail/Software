using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Ads.AdServer;

public class AdServerService : IAdService
{
	private readonly IAdServerClient _adServerClient;

	private readonly ILogger<AdServerService> _logger;

	private readonly IAssetService _assetService;

	private IApplicationConfiguration _config;

	private IOptionsSnapshotKioskConfiguration _kioskConfigiuration;

	private static Timer _timer;

	private static bool _inSession = false;

	private static ConcurrentDictionary<string, AdServerCachedAdvertisement> Cached { get; } = new ConcurrentDictionary<string, AdServerCachedAdvertisement>();

	public AdServerService(IAdServerClient adServerClient, ILogger<AdServerService> logger, IAssetService assetService, IApplicationConfiguration config, IOptionsSnapshotKioskConfiguration kioskConfigiuration)
	{
		_adServerClient = adServerClient;
		_logger = logger;
		_assetService = assetService;
		_config = config;
		_kioskConfigiuration = kioskConfigiuration;
	}

	public async Task<ApplicationResponse> StartApplication()
	{
		_logger.LogInfoWithSource("Starting service", "StartApplication");
		if (_timer == null)
		{
			_logger?.LogInfoWithSource("Creating and starting ad expire timer", "StartApplication");
			int dueTime = (int)TimeSpan.FromMinutes(3.0).TotalMilliseconds;
			int period = (int)TimeSpan.FromMinutes(3.0).TotalMilliseconds;
			_timer = new Timer(delegate
			{
				ExpireTimer();
			}, null, dueTime, period);
		}
		await Cache();
		return new ApplicationResponse();
	}

	public async Task<ApplicationResponse> EndApplication()
	{
		_logger?.LogInfoWithSource("Ending service", "EndApplication");
		await ExpireAll();
		return new ApplicationResponse();
	}

	public SessionResponse StartSession()
	{
		_inSession = true;
		_logger?.LogInfoWithSource("Starting session", "StartSession");
		PreLoadAds();
		return new SessionResponse();
	}

	public SessionResponse EndSession()
	{
		_inSession = false;
		_logger?.LogInfoWithSource("Ending session", "EndSession");
		return new SessionResponse();
	}

	public async Task<GetAdResponse> GetAd(GetAdRequest request)
	{
		try
		{
			GetAdResponse result = new GetAdResponse
			{
				AdLocation = request.AdLocation
			};
			if (!_kioskConfigiuration.KioskClientAds.AdsActive)
			{
				result.StatusCode = HttpStatusCode.ServiceUnavailable;
				return result;
			}
			AdServerCachedAdvertisement advertisement = GetAdvertisement(request.AdLocation);
			if (advertisement != null)
			{
				advertisement.Requested = true;
				result.CopyFrom(advertisement);
				return result;
			}
			if (!(await GetAdAndCache(request.AdLocation)))
			{
				result.StatusCode = HttpStatusCode.NotFound;
				return result;
			}
			advertisement = GetAdvertisement(request.AdLocation);
			if (advertisement != null)
			{
				advertisement.Requested = true;
				result.CopyFrom(advertisement);
				return result;
			}
			return result;
		}
		finally
		{
			PreLoadAds();
		}
	}

	public async Task<GetAdsResponse> GetAds(List<AdLocation> locations, bool isPreload = false)
	{
		try
		{
			_logger?.LogInfoWithSource(locations.ToJson(), "GetAds");
			GetAdsResponse result = new GetAdsResponse();
			if (!_kioskConfigiuration.KioskClientAds.AdsActive)
			{
				return result;
			}
			Dictionary<AdLocation, AdServerCachedAdvertisement> advertisements = GetAdvertisements(locations, isPreload);
			List<AdLocation> locationsToCache = new List<AdLocation>();
			foreach (AdLocation location in locations)
			{
				if (advertisements.TryGetValue(location, out var value))
				{
					value.Requested = !isPreload;
					result.GetAdResponses.Add(new GetAdResponse().CopyFrom(value));
				}
				else
				{
					locationsToCache.Add(location);
				}
			}
			if (locationsToCache.Any())
			{
				await GetAdsAndCache(locationsToCache);
				advertisements = GetAdvertisements(locationsToCache, isPreload);
				foreach (AdLocation item in locationsToCache)
				{
					if (advertisements.TryGetValue(item, out var value2))
					{
						value2.Requested = !isPreload;
						result.GetAdResponses.Add(new GetAdResponse().CopyFrom(value2));
					}
				}
			}
			result.StatusCode = (result.GetAdResponses.Any() ? HttpStatusCode.OK : HttpStatusCode.NotFound);
			return result;
		}
		finally
		{
			if (!isPreload)
			{
				PreLoadAds();
			}
		}
	}

	public async Task<GetAdsResponse> GetAds(string view, bool isPreload = false)
	{
		List<AdLocation> adLocations = GetAdLocations(view);
		if (!adLocations.Any())
		{
			return new GetAdsResponse
			{
				StatusCode = HttpStatusCode.NoContent
			};
		}
		return await GetAds(adLocations, isPreload);
	}

	public async Task<ProofOfPlayResponse> PostProofOfPlay(string advertisementId, ProofOfPlayRequest proofOfPlayRequest)
	{
		_logger?.LogInfoWithSource("Posting " + proofOfPlayRequest.ToJson(), "PostProofOfPlay");
		ProofOfPlayResponse result = new ProofOfPlayResponse();
		AdServerCachedAdvertisement advertisement = GetAdvertisement(advertisementId, remove: true);
		if (advertisement == null)
		{
			result.StatusCode = HttpStatusCode.NotFound;
			return result;
		}
		proofOfPlayRequest.AdvertisementId = advertisementId;
		APIResponse<object> aPIResponse = await _adServerClient.PostProofOfPlay(proofOfPlayRequest);
		aPIResponse.CopyTo(result);
		result.ResponseTime = aPIResponse.ResponseTime;
		return result;
	}

	public async Task<ExpireResponse> Expire(string advertisementId)
	{
		_logger?.LogInfoWithSource("Expiring " + advertisementId, "Expire");
		ExpireResponse result = new ExpireResponse();
		AdServerCachedAdvertisement advertisement = GetAdvertisement(advertisementId, remove: true);
		if (advertisement == null)
		{
			result.StatusCode = HttpStatusCode.NotFound;
			return result;
		}
		if (advertisement.Ad.Expirable)
		{
			APIResponse<object> aPIResponse = await _adServerClient.PostExpire(advertisementId);
			aPIResponse.CopyTo(result);
			result.ResponseTime = aPIResponse.ResponseTime;
		}
		return result;
	}

	public async Task<CacheResponse> Cache()
	{
		_logger?.LogInfoWithSource("Caching ads", "Cache");
		if (!_kioskConfigiuration.KioskClientAds.AdsActive)
		{
			return new CacheResponse
			{
				StatusCode = HttpStatusCode.ServiceUnavailable
			};
		}
		APIResponse<AdsResponse> aPIResponse = await _adServerClient.CacheAds(DisplayArea.AdLocations);
		CacheResponse cacheResponse = new CacheResponse();
		aPIResponse.CopyTo(cacheResponse);
		cacheResponse.ResponseTime = aPIResponse.ResponseTime;
		if (aPIResponse.IsSuccessStatusCode)
		{
			List<Ad> data = aPIResponse.Response.Data;
			if (data == null)
			{
				_logger.LogInfoWithSource("Cache response did not contain any assets", "Cache");
				return cacheResponse;
			}
			_assetService.ManageAssets(((IEnumerable<Ad>)data).Select((Func<Ad, BaseAsset>)((Ad a) => new AdServerAsset
			{
				AssetId = a.AssetId,
				AssetUrl = a.AssetUrl,
				MimeType = a.MimeType
			})).ToList());
			cacheResponse.AssetCacheCount = data.Count;
			return cacheResponse;
		}
		_logger.LogErrorWithSource("Did not cache ads successfully", "Cache");
		return cacheResponse;
	}

	private void PreLoadAds()
	{
		if (!_kioskConfigiuration.KioskClientAds.AdsActive && _inSession)
		{
			return;
		}
		DateTime now = DateTime.Now;
		if (now.Hour < 8 || now.Hour >= 23)
		{
			return;
		}
		Task.Run(delegate
		{
			Dictionary<AdLocation, AdServerCachedAdvertisement> advertisements = GetAdvertisements(_config.AdsToPreload, isPreload: true);
			List<AdLocation> list = _config.AdsToPreload.Except(advertisements.Keys).ToList();
			if (list.Any())
			{
				GetAdsAndCache(list);
			}
		});
	}

	private async Task<bool> GetAdsAndCache(List<AdLocation> adLocations)
	{
		_logger?.LogInfoWithSource(adLocations.ToJson(), "GetAdsAndCache");
		APIResponse<AdsResponse> aPIResponse = await _adServerClient.GetAds(adLocations);
		if (!aPIResponse.IsSuccessStatusCode)
		{
			return false;
		}
		List<Ad> data = aPIResponse.Response.Data;
		data.ForEach(delegate(Ad a)
		{
			AdServerCachedAdvertisement adServerCachedAdvertisement = new AdServerCachedAdvertisement
			{
				Ad = a,
				RequestedTime = DateTime.Now
			};
			Asset asset = _assetService.GetAsset(adServerCachedAdvertisement.Ad.AssetId);
			if (asset == null)
			{
				_logger?.LogErrorWithSource("DownloadAd succeeded but asset was not cached. Expiring get ad request now.", "GetAdsAndCache");
				if (a.Expirable)
				{
					_adServerClient.PostExpire(a.Id);
				}
				_assetService.DownloadAsset(adServerCachedAdvertisement.Ad.ToAdServerAsset());
			}
			else
			{
				adServerCachedAdvertisement.AssetPath = _assetService.GetAssetCreativeFilePath(asset);
				AddAdvertisement(adServerCachedAdvertisement);
			}
		});
		return true;
	}

	private Dictionary<AdLocation, AdServerCachedAdvertisement> GetAdvertisements(List<AdLocation> adLocations, bool isPreload = false)
	{
		Dictionary<AdLocation, AdServerCachedAdvertisement> result = new Dictionary<AdLocation, AdServerCachedAdvertisement>();
		adLocations.ForEach(delegate(AdLocation location)
		{
			List<AdServerCachedAdvertisement> list = Cached.Values.Where((AdServerCachedAdvertisement ca) => ca.Ad.Location == location).ToList();
			AdServerCachedAdvertisement adServerCachedAdvertisement = (from ca in list
				where !ca.Requested || (DateTime.Now - ca.RequestedTime).TotalSeconds >= 120.0
				orderby ca.RequestedTime
				select ca).FirstOrDefault();
			if (adServerCachedAdvertisement == null && !isPreload)
			{
				list.ForEach(delegate(AdServerCachedAdvertisement ca)
				{
					ca.Requested = false;
				});
			}
			else if (adServerCachedAdvertisement != null)
			{
				adServerCachedAdvertisement.Requested = true;
				result[location] = adServerCachedAdvertisement;
			}
		});
		return result;
	}

	private void AddAdvertisement(AdServerCachedAdvertisement ca)
	{
		Cached[ca.Ad.Id] = ca;
	}

	private async Task<bool> ExpireAll()
	{
		_logger?.LogInfoWithSource("Expiring all ads", "ExpireAll");
		List<Task> list = new List<Task>();
		foreach (AdServerCachedAdvertisement item in Cached.Values.ToList())
		{
			list.Add(ExpireAdAndRemoveFromCache(item.Ad.Id));
		}
		await Task.WhenAll(list);
		Cached.Clear();
		return true;
	}

	private async Task<bool> ExpireAdAndRemoveFromCache(string advertisementId)
	{
		_logger?.LogInfoWithSource("Expiring " + advertisementId, "ExpireAdAndRemoveFromCache");
		AdServerCachedAdvertisement advertisement = GetAdvertisement(advertisementId, remove: true);
		if (advertisement == null)
		{
			return false;
		}
		if (advertisement.Ad.Expirable)
		{
			await _adServerClient.PostExpire(advertisementId);
		}
		return true;
	}

	private AdServerCachedAdvertisement GetAdvertisement(string advertisementId, bool remove)
	{
		AdServerCachedAdvertisement value;
		if (remove)
		{
			Cached.TryRemove(advertisementId, out value);
		}
		else
		{
			Cached.TryGetValue(advertisementId, out value);
		}
		return value;
	}

	private AdServerCachedAdvertisement GetAdvertisement(AdLocation adLocation, bool isPreload = false)
	{
		List<AdServerCachedAdvertisement> list = Cached.Values.Where((AdServerCachedAdvertisement x) => x.Ad.Location == adLocation).ToList();
		AdServerCachedAdvertisement adServerCachedAdvertisement = (from x in list
			where !x.Requested || (DateTime.Now - x.RequestedTime).TotalSeconds >= 120.0
			orderby x.RequestedTime
			select x).FirstOrDefault();
		if (adServerCachedAdvertisement == null && !isPreload)
		{
			list.ForEach(delegate(AdServerCachedAdvertisement x)
			{
				x.Requested = false;
			});
			return null;
		}
		return adServerCachedAdvertisement;
	}

	private async Task<bool> GetAdAndCache(AdLocation adLocation)
	{
		APIResponse<AdResponse> aPIResponse = await _adServerClient.GetAd(adLocation);
		if (!aPIResponse.IsSuccessStatusCode)
		{
			return false;
		}
		bool result = false;
		Ad data = aPIResponse.Response.Data;
		AdServerCachedAdvertisement cached = new AdServerCachedAdvertisement
		{
			Ad = data,
			RequestedTime = DateTime.Now
		};
		Asset asset = _assetService.GetAsset(cached.Ad.AssetId);
		if (asset == null)
		{
			_logger?.LogError("DownloadAd succeeded but asset was not cached.  Expiring get ad request now.");
			if (data.Expirable)
			{
				await _adServerClient.PostExpire(data.Id);
			}
			_assetService.DownloadAsset(cached.Ad.ToAdServerAsset());
		}
		else
		{
			cached.AssetPath = _assetService.GetAssetCreativeFilePath(asset);
			AddAdvertisement(cached);
			result = true;
		}
		return result;
	}

	private List<AdLocation> GetAdLocations(string view)
	{
		_config.AdsToPreloadByView.TryGetValue(view, out var value);
		return value ?? new List<AdLocation>();
	}

	private async Task ExpireTimer()
	{
		await ExpireCachedAds();
	}

	private async Task ExpireCachedAds()
	{
		DateTime age = DateTime.Now.AddMinutes(-120.0);
		List<AdServerCachedAdvertisement> list = (from x in Cached.Values
			where x.RequestedTime < age
			orderby x.RequestedTime
			select x).ToList();
		if (list.Count == 0)
		{
			return;
		}
		_logger?.LogInfoWithSource($"Found {list.Count} ads to expire", "ExpireCachedAds");
		List<Task> list2 = new List<Task>();
		foreach (AdServerCachedAdvertisement item in list)
		{
			list2.Add(ExpireAdAndRemoveFromCache(item.Ad.Id));
			_logger?.LogInfoWithSource($"Expiring {item.Ad.Id}, requested {item.Requested}, cached {item.RequestedTime}", "ExpireCachedAds");
		}
		await Task.WhenAll(list2);
	}
}
