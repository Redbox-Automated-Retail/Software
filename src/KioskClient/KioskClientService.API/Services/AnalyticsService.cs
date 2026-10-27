using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.Analytics;
using KioskClientService.API.Services.Analytics.KioskDataService;
using KioskClientService.API.Services.Analytics.Vistar;
using KioskClientService.API.Services.Analytics.Vistar.Request;
using KioskClientService.API.Services.Analytics.Vistar.Summaries;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Vistar;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services;

public class AnalyticsService : IAnalyticsService
{
	private readonly ILogger<AnalyticsService> _logger;

	private readonly IAdServerAnalyticsClient _adServerClient;

	private readonly IKioskDataServiceClient _kdsClient;

	private readonly IStoreService _store;

	private readonly IAnalyticsQueue _queue;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly Guid _clientSessionId = Guid.NewGuid();

	public AnalyticsService(IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, ILogger<AnalyticsService> logger, IStoreService store, IAdServerAnalyticsClient adServerClient, IKioskDataServiceClient kdsClient, IAnalyticsQueue queue)
	{
		_logger = logger;
		_adServerClient = adServerClient;
		_kdsClient = kdsClient;
		_store = store;
		_queue = queue;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task ServiceEvent(KioskClientEvent eventType, DateTime? eventTime)
	{
		_logger.LogInfoWithSource($"AnalyticsService.{eventType}", "ServiceEvent");
		try
		{
			KioskClientEventRequest request = new KioskClientEventRequest(_store.KioskId, eventType, eventTime);
			await _kdsClient.PostKioskClientEvent(request);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"AnalyticsService.{eventType} - unhandled exception occurred. {arg}", "ServiceEvent");
		}
	}

	public async Task VistarGetAd(AdLocation adLocation, APIResponse<VistarGetAdResponse> getAdResponse)
	{
		_logger.LogInfoWithSource("AnalyticsService.VistarGetAd", "VistarGetAd");
		try
		{
			GetAdAnalyticsRequest analyticsObject = new GetAdAnalyticsRequest
			{
				KioskId = _store.KioskId,
				StatusCode = getAdResponse.StatusCode,
				ResponseTime = getAdResponse.ResponseTime,
				AdLocation = adLocation,
				Success = getAdResponse.IsSuccessStatusCode
			};
			await HandleAnalytics(AnalyticsType.VistarGetAd, analyticsObject);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"AnlyticsService.VistarGetAd - unhandled exception occurred. {arg}", "VistarGetAd");
		}
	}

	public async Task VistarProofOfPlay(ProofOfPlayRequest proofOfPlayRequest, ProofOfPlayResponse proofOfPlayResponse, CachedAdvertisement ad = null)
	{
		_logger.LogInfoWithSource("AnalyticsService.VistarProofOfPlay", "VistarProofOfPlay");
		try
		{
			ProofOfPlayAnalyticsRequest analyticsObject = new ProofOfPlayAnalyticsRequest
			{
				KioskId = _store.KioskId,
				StatusCode = proofOfPlayResponse.StatusCode,
				ResponseTime = proofOfPlayResponse.ResponseTime,
				DisplayedMilliseconds = proofOfPlayRequest.DisplayedMilliseconds,
				DisplayedTime = proofOfPlayRequest.DisplayedTime,
				Success = (proofOfPlayResponse.StatusCode == HttpStatusCode.OK),
				AdLocation = (ad?.AdLocation ?? AdLocation.Unknown)
			};
			await HandleAnalytics(AnalyticsType.VistarProofOfPlay, analyticsObject);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"AnalyticsService.VistarProofOfPlay - unhandled exception occurred. {arg}", "VistarProofOfPlay");
		}
	}

	public async Task VistarExpire(VistarExpirationRequest expireRequest, ExpireResponse expireResponse, CachedAdvertisement ad, AdExpirationSource adExpirationSource)
	{
		_logger.LogInfoWithSource("AnalyticsService.VistarExpire", "VistarExpire");
		try
		{
			ExpireAnalyticsRequest analyticsObject = new ExpireAnalyticsRequest
			{
				KioskId = _store.KioskId,
				StatusCode = expireResponse.StatusCode,
				ResponseTime = expireResponse.ResponseTime,
				Requested = ad?.Requested,
				AdLocation = (ad?.AdLocation ?? AdLocation.Unknown),
				AdExpirationSource = adExpirationSource,
				Success = (expireResponse.StatusCode == HttpStatusCode.OK)
			};
			await HandleAnalytics(AnalyticsType.VistarExpire, analyticsObject);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"AnalyticsService.VistarExpire - unhandled exception occurred. {arg}", "VistarExpire");
		}
	}

	public async Task VistarCache(GetAssetsRequest getAssetsRequest, CacheResponse cacheResponse)
	{
		try
		{
			_logger.LogInfoWithSource("AnalyticsService.VistarCache", "VistarCache");
			CacheAnalyticsRequest analyticsObject = new CacheAnalyticsRequest
			{
				KioskId = _store.KioskId,
				StatusCode = cacheResponse.StatusCode,
				ResponseTime = cacheResponse.ResponseTime,
				AssetsCached = cacheResponse.AssetCacheCount,
				Success = (cacheResponse.StatusCode == HttpStatusCode.OK)
			};
			await HandleAnalytics(AnalyticsType.VistarCache, analyticsObject);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"AnalyticsService.Cache - unhandled exception occurred. {arg}", "VistarCache");
		}
	}

	public async Task DownloadAsset(APIResponse<byte[]> downloadAssetResponse)
	{
		_logger.LogInfoWithSource("AnalyticsService.DownloadAsset", "DownloadAsset");
		try
		{
			DownloadAssetAnalyticsRequest analyticsObject = new DownloadAssetAnalyticsRequest
			{
				KioskId = _store.KioskId,
				BytesDownloaded = downloadAssetResponse.ResponseLength,
				ResponseTime = downloadAssetResponse.ResponseTime,
				ResponseLength = downloadAssetResponse.ResponseLength,
				StatusCode = downloadAssetResponse.StatusCode,
				Success = downloadAssetResponse.IsSuccessStatusCode
			};
			await HandleAnalytics(AnalyticsType.VistarDownloadAsset, analyticsObject);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"AnalyticsService.DownloadAsset - unhandled exception occurred. {arg}", "DownloadAsset");
		}
	}

	public async Task GetAdTimeoutEvent(GetAdTimeoutEvent timeoutEvent)
	{
		_logger.LogInfoWithSource("", "GetAdTimeoutEvent");
		try
		{
			await HandleAnalytics(AnalyticsType.GetAdTimeoutEvent, timeoutEvent);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "unhandled exception occurred.", "GetAdTimeoutEvent");
		}
	}

	public void PurgeOldAnalytics(TimeSpan age)
	{
		_logger.LogInfoWithSource($"AnalyticsService.PurgeOldAnalytics - purging files older than {DateTime.Now - age}", "PurgeOldAnalytics");
		try
		{
			string path = Path.Combine(_store.DataPath, "Analytics");
			IEnumerable<string> enumerable = from file in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories)
				where DateTime.Now - File.GetLastWriteTime(file) > age
				select file;
			foreach (string item in enumerable)
			{
				File.Delete(item);
			}
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"AnalyticsService.PurgeOldAnalytics - unhandled exception occurred. {arg}", "PurgeOldAnalytics");
		}
	}

	public void PurgeServiceEvents()
	{
		_logger.LogInfoWithSource("purging saved service events", "PurgeServiceEvents");
		try
		{
			string path = Path.Combine(_store.DataPath, "Analytics", AnalyticsType.ServiceEvent.ToString());
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"unhandled exception occurred. {arg}", "PurgeServiceEvents");
		}
	}

	public void SaveServiceEvent(ServiceEvent serviceEvent)
	{
		try
		{
			string text = serviceEvent.ToJson();
			_logger.LogInfoWithSource("saving event " + text, "SaveServiceEvent");
			string text2 = Path.Combine(_store.DataPath, "Analytics", AnalyticsType.ServiceEvent.ToString());
			Directory.CreateDirectory(text2);
			string path = Path.Combine(text2, serviceEvent.EventType.ToString() + ".json");
			File.WriteAllText(path, text);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"unhandled exception occurred. {arg}", "SaveServiceEvent");
		}
	}

	public ServiceEvent LoadServiceEvent(KioskClientEvent eventType)
	{
		try
		{
			_logger.LogInfoWithSource($"loading {eventType} event", "LoadServiceEvent");
			string path = Path.Combine(_store.DataPath, "Analytics", AnalyticsType.ServiceEvent.ToString(), eventType.ToString() + ".json");
			string value = null;
			if (File.Exists(path))
			{
				value = File.ReadAllText(path);
			}
			if (!string.IsNullOrWhiteSpace(value))
			{
				return JsonConvert.DeserializeObject<ServiceEvent>(value);
			}
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"error while loading {eventType} event. {arg}", "LoadServiceEvent");
		}
		return null;
	}

	private async Task HandleAnalytics(AnalyticsType type, object analyticsObject)
	{
		if (_kioskConfiguration.KioskClientAds.AdAnalyticsActive)
		{
			await _queue.Enqueue(type, async delegate
			{
				string fullPath = Path.GetFullPath(Path.Combine(_store.DataPath, "Analytics/" + type));
				Directory.CreateDirectory(Path.Combine(fullPath, "Complete"));
				Directory.CreateDirectory(Path.Combine(fullPath, "Error"));
				DirectoryInfo pendingDirectory = Directory.CreateDirectory(Path.Combine(fullPath, "Pending"));
				string filePath = FilePath(fullPath, type, DateTime.Now);
				_logger.LogInfoWithSource("Analytics.HandleAnalytics - writing " + type.ToString() + " analytics to " + filePath + ": " + analyticsObject.ToJson(), "HandleAnalytics");
				try
				{
					if (!File.Exists(filePath))
					{
						List<string> source = Directory.GetFiles(Directory.GetParent(filePath).FullName).ToList();
						await Task.WhenAll(((IEnumerable<string>)source).Select((Func<string, Task>)async delegate(string path)
						{
							await CompleteAnalyticsFile(path, type);
						}));
						using StreamWriter file = File.AppendText(filePath);
						await file.WriteAsync("[" + JsonConvert.SerializeObject(analyticsObject));
					}
					else
					{
						using StreamWriter file = File.AppendText(filePath);
						await file.WriteAsync("," + JsonConvert.SerializeObject(analyticsObject));
					}
				}
				catch (Exception arg)
				{
					_logger.LogErrorWithSource($"Analytics.HandleAnalytics - error while writing analytics for {filePath}. {arg}", "HandleAnalytics");
				}
				List<string> list = Directory.GetFiles(pendingDirectory.FullName).ToList();
				foreach (string item in list)
				{
					await SendAnalytics(item, type);
				}
			});
		}
		else
		{
			_logger.LogInfoWithSource($"Analytics type {type} not processed because AdAnalyticsActive configuration is turned off", "HandleAnalytics");
		}
	}

	private async Task CompleteAnalyticsFile(string filePath, AnalyticsType type)
	{
		try
		{
			if (File.Exists(filePath))
			{
				string pendingPath = Path.Combine(Directory.GetParent(filePath).FullName, "Pending");
				using (StreamWriter file = File.AppendText(filePath))
				{
					await file.WriteAsync(']');
				}
				_logger.LogInfoWithSource("Analytics.CompleteAnalyticsFile - moving " + Path.GetFileName(filePath) + " to " + pendingPath, "CompleteAnalyticsFile");
				File.Move(filePath, Path.Combine(pendingPath, Path.GetFileName(filePath)));
			}
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"Analytics.CompleteAnalyticsFile - error completing file at {filePath}. {arg}", "CompleteAnalyticsFile");
		}
	}

	private async Task SendAnalytics(string filePath, AnalyticsType type)
	{
		if (!File.Exists(filePath))
		{
			return;
		}
		try
		{
			_logger.LogInfoWithSource("Analytics.SendAnalytics: Sending " + Path.GetFileName(filePath), "SendAnalytics");
			APIResponse<AnalyticsResponse> aPIResponse = null;
			switch (type)
			{
			case AnalyticsType.VistarGetAd:
			{
				VistarGetAdSummary request6 = AggregateVistarGetAd(filePath);
				aPIResponse = await _adServerClient.PostGetAdAnalytics(request6);
				break;
			}
			case AnalyticsType.VistarProofOfPlay:
			{
				VistarProofOfPlaySummary request5 = AggregateVistarProofOfPlay(filePath);
				aPIResponse = await _adServerClient.PostProofOfPlayAnalytics(request5);
				break;
			}
			case AnalyticsType.VistarExpire:
			{
				VistarExpireSummary request4 = AggregateVistarExpire(filePath);
				aPIResponse = await _adServerClient.PostExpireAnalytics(request4);
				break;
			}
			case AnalyticsType.VistarCache:
			{
				VistarCacheSummary request3 = AggregateVistarCache(filePath);
				aPIResponse = await _adServerClient.PostCacheAnalytics(request3);
				break;
			}
			case AnalyticsType.VistarDownloadAsset:
			{
				VistarDownloadAssetSummary request2 = AggregateVistarDownloadAsset(filePath);
				aPIResponse = await _adServerClient.PostDownloadAssetAnalytics(request2);
				break;
			}
			case AnalyticsType.GetAdTimeoutEvent:
			{
				GetAdTimeoutEventSummary request = AggregateGetAdTimeoutEvent(filePath);
				aPIResponse = await _adServerClient.PostGetAdTimeoutEventAnalytics(request);
				break;
			}
			default:
				_logger.LogInfoWithSource("Analytics.SendAnalytics: Aggregation for analytics type " + type.ToString() + " not implemented.", "SendAnalytics");
				break;
			}
			if (aPIResponse != null && aPIResponse.IsSuccessStatusCode)
			{
				File.Move(filePath, Path.Combine(Directory.GetParent(filePath).Parent.FullName, "Complete/" + Path.GetFileName(filePath)));
			}
			else
			{
				MoveToErrorFolder(filePath, new Exception("Analytics.SendAnalytics: An error occurred. " + aPIResponse?.ResponseContent));
			}
		}
		catch (UnauthorizedAccessException arg)
		{
			_logger.LogErrorWithSource($"Analytics.SendAnalytics: Error for {Path.GetFileName(filePath)}. {arg}", "SendAnalytics");
		}
		catch (Exception error)
		{
			MoveToErrorFolder(filePath, error);
		}
	}

	private void MoveToErrorFolder(string filePath, Exception error)
	{
		try
		{
			if (File.Exists(filePath))
			{
				File.Move(filePath, Path.Combine(Directory.GetParent(filePath).Parent.FullName, "Error/" + Path.GetFileName(filePath)));
			}
			_logger.LogErrorWithSource($"Analytics.SendAnalytics: Error for {Path.GetFileName(filePath)}. {error}", "MoveToErrorFolder");
		}
		catch (Exception arg)
		{
			_logger.LogError($"Analytics.MoveToErrorFolder: Error for {Path.GetFileName(filePath)}. {arg}");
		}
	}

	private VistarGetAdSummary AggregateVistarGetAd(string filePath)
	{
		List<GetAdAnalyticsRequest> list = FileToList<GetAdAnalyticsRequest>(filePath);
		List<IGrouping<AdLocation, GetAdAnalyticsRequest>> source = (from d in list
			group d by d.AdLocation).ToList();
		VistarGetAdSummary vistarGetAdSummary = new VistarGetAdSummary
		{
			Type = AnalyticsType.VistarGetAd,
			MedianResponseTimeByLocation = source.ToDictionary((IGrouping<AdLocation, GetAdAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, GetAdAnalyticsRequest> group) => group.Median((GetAdAnalyticsRequest d) => d.ResponseTime)),
			MaxResponseTimeByLocation = source.ToDictionary((IGrouping<AdLocation, GetAdAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, GetAdAnalyticsRequest> group) => group.Max((GetAdAnalyticsRequest d) => d.ResponseTime)),
			SuccessRateByLocation = source.ToDictionary((IGrouping<AdLocation, GetAdAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, GetAdAnalyticsRequest> group) => Rate(group.Count((GetAdAnalyticsRequest d) => d.Success), group.Count()))
		};
		AggregateHttpSummary(list, vistarGetAdSummary);
		return vistarGetAdSummary;
	}

	private VistarProofOfPlaySummary AggregateVistarProofOfPlay(string filePath)
	{
		List<ProofOfPlayAnalyticsRequest> list = FileToList<ProofOfPlayAnalyticsRequest>(filePath);
		List<IGrouping<AdLocation, ProofOfPlayAnalyticsRequest>> source = (from d in list
			group d by d.AdLocation).ToList();
		VistarProofOfPlaySummary vistarProofOfPlaySummary = new VistarProofOfPlaySummary
		{
			Type = AnalyticsType.VistarProofOfPlay,
			MedianDisplayedDuration = list.Median((ProofOfPlayAnalyticsRequest d) => d.DisplayedMilliseconds),
			MaxDisplayedDuration = list.Max((ProofOfPlayAnalyticsRequest d) => d.DisplayedMilliseconds),
			MinDisplayedDuration = list.Min((ProofOfPlayAnalyticsRequest d) => d.DisplayedMilliseconds),
			MedianResponseTimeByLocation = source.ToDictionary((IGrouping<AdLocation, ProofOfPlayAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, ProofOfPlayAnalyticsRequest> group) => group.Median((ProofOfPlayAnalyticsRequest d) => d.ResponseTime)),
			MaxResponseTimeByLocation = source.ToDictionary((IGrouping<AdLocation, ProofOfPlayAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, ProofOfPlayAnalyticsRequest> group) => group.Max((ProofOfPlayAnalyticsRequest d) => d.ResponseTime)),
			SuccessRateByLocation = source.ToDictionary((IGrouping<AdLocation, ProofOfPlayAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, ProofOfPlayAnalyticsRequest> group) => Rate(group.Count((ProofOfPlayAnalyticsRequest d) => d.Success), group.Count()))
		};
		AggregateHttpSummary(list, vistarProofOfPlaySummary);
		return vistarProofOfPlaySummary;
	}

	private VistarExpireSummary AggregateVistarExpire(string filePath)
	{
		List<ExpireAnalyticsRequest> list = FileToList<ExpireAnalyticsRequest>(filePath);
		List<IGrouping<AdLocation, ExpireAnalyticsRequest>> source = (from d in list
			group d by d.AdLocation).ToList();
		List<ExpireAnalyticsRequest> list2 = list.Where((ExpireAnalyticsRequest d) => d.AdExpirationSource == AdExpirationSource.ClientService).ToList();
		VistarExpireSummary vistarExpireSummary = new VistarExpireSummary
		{
			Type = AnalyticsType.VistarExpire,
			ExpiredByServiceCount = list2.Count,
			ExpireHealth = Rate(list2.Count((ExpireAnalyticsRequest e) => e.Requested == true), list2.Count),
			MedianResponseTimeByLocation = source.ToDictionary((IGrouping<AdLocation, ExpireAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, ExpireAnalyticsRequest> group) => group.Median((ExpireAnalyticsRequest d) => d.ResponseTime)),
			MaxResponseTimeByLocation = source.ToDictionary((IGrouping<AdLocation, ExpireAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, ExpireAnalyticsRequest> group) => group.Max((ExpireAnalyticsRequest d) => d.ResponseTime)),
			SuccessRateByLocation = source.ToDictionary((IGrouping<AdLocation, ExpireAnalyticsRequest> group) => group.Key.ToString(), (IGrouping<AdLocation, ExpireAnalyticsRequest> group) => Rate(group.Count((ExpireAnalyticsRequest d) => d.Success), group.Count()))
		};
		AggregateHttpSummary(list, vistarExpireSummary);
		return vistarExpireSummary;
	}

	private VistarCacheSummary AggregateVistarCache(string filePath)
	{
		List<CacheAnalyticsRequest> list = FileToList<CacheAnalyticsRequest>(filePath);
		VistarCacheSummary vistarCacheSummary = new VistarCacheSummary
		{
			Type = AnalyticsType.VistarCache,
			MedianAssetsCached = list.Median((CacheAnalyticsRequest d) => d.AssetsCached),
			MaxAssetsCached = list.Max((CacheAnalyticsRequest d) => d.AssetsCached),
			MinAssetsCached = list.Min((CacheAnalyticsRequest d) => d.AssetsCached)
		};
		AggregateHttpSummary(list, vistarCacheSummary);
		return vistarCacheSummary;
	}

	private VistarDownloadAssetSummary AggregateVistarDownloadAsset(string filePath)
	{
		List<DownloadAssetAnalyticsRequest> list = FileToList<DownloadAssetAnalyticsRequest>(filePath);
		VistarDownloadAssetSummary vistarDownloadAssetSummary = new VistarDownloadAssetSummary
		{
			Type = AnalyticsType.VistarCache,
			TotalBytesDownloaded = list.Sum((DownloadAssetAnalyticsRequest d) => d.BytesDownloaded),
			MaxBytesDownloaded = list.Max((DownloadAssetAnalyticsRequest d) => d.BytesDownloaded),
			MedianBytesDownloaded = list.Median((DownloadAssetAnalyticsRequest d) => d.BytesDownloaded)
		};
		AggregateHttpSummary(list, vistarDownloadAssetSummary);
		return vistarDownloadAssetSummary;
	}

	private GetAdTimeoutEventSummary AggregateGetAdTimeoutEvent(string filePath)
	{
		List<GetAdTimeoutEvent> list = FileToList<GetAdTimeoutEvent>(filePath);
		IEnumerable<IGrouping<AdLocation, GetAdTimeoutEvent>> source = from d in list
			group d by d.AdLocation;
		IEnumerable<IGrouping<int, GetAdTimeoutEvent>> source2 = from d in list
			group d by d.CreatedOn.Hour;
		GetAdTimeoutEventSummary getAdTimeoutEventSummary = new GetAdTimeoutEventSummary
		{
			Type = AnalyticsType.GetAdTimeoutEvent,
			RequestsByAdLocation = source.ToDictionary((IGrouping<AdLocation, GetAdTimeoutEvent> d) => d.Key, (IGrouping<AdLocation, GetAdTimeoutEvent> d) => d.Count()),
			RequestsByHour = source2.ToDictionary((IGrouping<int, GetAdTimeoutEvent> d) => d.Key, (IGrouping<int, GetAdTimeoutEvent> d) => d.Count())
		};
		AggregateSummary(list, getAdTimeoutEventSummary);
		return getAdTimeoutEventSummary;
	}

	private void AggregateHttpSummary<T, TU>(List<T> data, TU summary) where T : HttpAnalyticsRequest where TU : HttpSummary
	{
		AggregateSummary(data, summary);
		summary.MedianResponseTime = data.Median((T d) => d.ResponseTime);
		summary.MaxResponseTime = data.Max((T d) => d.ResponseTime);
		summary.SuccessRate = Rate(data.Count((T d) => d.Success), data.Count);
	}

	private void AggregateSummary<T, TU>(List<T> data, TU summary) where T : BaseAnalyticsRequest where TU : Summary
	{
		summary.KioskId = _store.KioskId;
		summary.SummaryDate = data.First().LocalTime.Date;
		summary.Requests = data.Count;
	}

	private static string FilePath(string dataPath, AnalyticsType type, DateTime date)
	{
		string text = "yyyyMMdd";
		return Path.GetFullPath(Path.Combine(dataPath, type.ToString() + "-" + date.ToString(text) + ".json"));
	}

	private static double? Rate(int matches, int total)
	{
		if (total == 0)
		{
			return null;
		}
		return (double)matches / (double)total;
	}

	private List<T> FileToList<T>(string filePath)
	{
		List<T> list = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath));
		if (list == null || list.Count == 0)
		{
			throw new Exception("No data found for file " + filePath);
		}
		return list;
	}
}
