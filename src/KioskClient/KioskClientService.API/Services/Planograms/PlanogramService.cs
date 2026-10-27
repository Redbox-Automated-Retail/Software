using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Planograms;

public class PlanogramService : IPlanogramService
{
	private class SelectedPlanogramInfo
	{
		public string Name { get; set; }
	}

	private readonly IApplicationSettings _settings;

	private readonly IProxyApi _proxyApi;

	private readonly IStoreService _storeService;

	private readonly IHttpService _httpService;

	private readonly ILogger<IPlanogramService> _logger;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly string _planogramPath;

	private const string CACHE_FILE_NAME = "CurrentPlanograms";

	private const string SELECTED_PLANOGRAM_FILENAME = "selected_planogram.json";

	private const int DOWNLOAD_RETRY_DELAY_MINUTES = 5;

	private static int _updatePeriod = 3600;

	private static int _updateSeconds = new Random().Next(0, _updatePeriod - 300);

	private static DateTime? _lastSync = null;

	private static object _syncObject = new object();

	public PlanogramService(IApplicationSettings settings, IProxyApi proxyApi, IStoreService storeService, IHttpService httpService, IPersistentDataCacheService cache, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, ILogger<IPlanogramService> logger)
	{
		_settings = settings;
		_proxyApi = proxyApi;
		_storeService = storeService;
		_httpService = httpService;
		_persistentDataCacheService = cache;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_logger = logger;
		_planogramPath = Path.Combine(_storeService.DataPath, _settings.AppSettings.PlanogramFolder);
	}

	public List<Planogram> GetValidPlanograms(string filenameSearch = "*")
	{
		List<Planogram> list = new List<Planogram>();
		CheckPlanogramPath(_planogramPath);
		List<string> list2 = Directory.GetFiles(_planogramPath, filenameSearch + ".xps").ToList();
		foreach (string item in list2)
		{
			Planogram planogram = Planogram.Create(item, _logger);
			if (planogram != null)
			{
				list.Add(planogram);
			}
		}
		return list;
	}

	public List<Planogram> GetActivePlanograms(string filenameSearch = "*")
	{
		List<Planogram> list = new List<Planogram>();
		if (_kioskConfiguration.KioskClientDefault.PlanogramSyncEnabled)
		{
			List<Planogram> validPlanograms = GetValidPlanograms(filenameSearch);
			IEnumerable<IGrouping<string, Planogram>> enumerable = from p in validPlanograms
				where p.StartDate <= DateTime.Now
				group p by p.Name.ToLower().Trim();
			foreach (IGrouping<string, Planogram> item in enumerable)
			{
				list.Add(item.OrderByDescending((Planogram p) => p.StartDate).First());
			}
		}
		else
		{
			_logger.LogInfoWithSource("GetActivePlanograms called but planogram sync is not enabled in the config. No planograms will be returned.", "GetActivePlanograms");
		}
		return list;
	}

	public async Task<Planogram> GetSelectedPlanogram()
	{
		Planogram result = null;
		SelectedPlanogramInfo selectedPlanogramInfo = await GetSelectedPlanogramInfo();
		if (!string.IsNullOrWhiteSpace(selectedPlanogramInfo.Name))
		{
			List<Planogram> activePlanograms = GetActivePlanograms(selectedPlanogramInfo.Name + "_*");
			result = activePlanograms.FirstOrDefault();
		}
		return result;
	}

	public async Task<SavePlanogramInfoResponse> SaveSelectedPlanogram(string planogramName)
	{
		List<Planogram> validPlanograms = GetValidPlanograms(planogramName + "_*");
		if (validPlanograms.Count == 0)
		{
			SavePlanogramInfoResponse savePlanogramInfoResponse = new SavePlanogramInfoResponse
			{
				StatusCode = HttpStatusCode.BadRequest
			};
			savePlanogramInfoResponse.Error = "No valid planograms exist matching the name " + planogramName;
			return savePlanogramInfoResponse;
		}
		SelectedPlanogramInfo info = new SelectedPlanogramInfo
		{
			Name = planogramName
		};
		return await SaveSelectedPlanogramInfo(info);
	}

	public async Task<SyncPlanogramsResponse> SyncPlanograms(bool forceDownload = false)
	{
		SyncPlanogramsResponse result = new SyncPlanogramsResponse();
		SyncPlanogramInfo lastResult = await GetSyncInfo();
		APIResponse<GetPlanogramsResponse> aPIResponse = await _proxyApi.GetPlanograms(_storeService.KioskId, forceDownload ? ((DateTime?)null) : lastResult?.LatestChange);
		if (!aPIResponse.IsSuccessStatusCode || aPIResponse.Response == null || aPIResponse.Response.Planograms == null)
		{
			result.StatusCode = HttpStatusCode.InternalServerError;
			string text = aPIResponse.Response?.Error ?? ((aPIResponse.Errors == null) ? null : aPIResponse.Errors.FirstOrDefault()?.Message) ?? "Error retreiving Planograms from server";
			_logger.LogErrorWithSource($"Unable to retrieve plangoram list from the server, status code: {aPIResponse.StatusCode}, error: {text}", "SyncPlanograms");
			result.Error = text;
			return result;
		}
		SyncPlanogramInfo syncInfo = null;
		if (aPIResponse.StatusCode == HttpStatusCode.AlreadyReported)
		{
			_logger.LogInfoWithSource($"No new planogram data was returned for last change date {lastResult?.LatestChange}", "SyncPlanograms");
		}
		else
		{
			List<PlanogramInfo> planograms = aPIResponse.Response.Planograms;
			DateTime latestChange = planograms.Max((PlanogramInfo p) => p.LastUpdateUtc);
			syncInfo = new SyncPlanogramInfo
			{
				ForceDownload = forceDownload,
				LatestChange = latestChange,
				Planograms = planograms
			};
		}
		lock (_syncObject)
		{
			_lastSync = DateTime.Now;
		}
		bool flag = syncInfo == null && !lastResult.SyncComplete;
		bool flag2 = flag;
		if (!flag2)
		{
			bool flag3 = syncInfo != null;
			bool flag4 = flag3;
			if (flag4)
			{
				flag4 = await _persistentDataCacheService.Write(syncInfo, "CurrentPlanograms");
			}
			flag2 = flag4;
		}
		if (flag2)
		{
			Task.Run(async delegate
			{
				await SyncFilesIfNeeded().ConfigureAwait(continueOnCapturedContext: false);
			});
		}
		else if (syncInfo != null)
		{
			_logger.LogErrorWithSource("Unable to store sync info to the persistent data cache.", "SyncPlanograms");
			result.Error = "Error storing sync data";
			result.StatusCode = HttpStatusCode.InternalServerError;
		}
		return result;
	}

	public async Task SyncFilesIfNeeded(int retryCount = 0)
	{
		try
		{
			SyncPlanogramInfo syncInfo = await GetSyncInfo();
			if (syncInfo.SyncComplete)
			{
				return;
			}
			CheckPlanogramPath(_planogramPath);
			DirectoryInfo directoryInfo = new DirectoryInfo(_planogramPath);
			List<FileInfo> localFiles = directoryInfo.GetFiles("*.xps").ToList();
			List<PlanogramInfo> serverPlanograms = syncInfo.Planograms;
			bool forceDownload = syncInfo.ForceDownload;
			List<PlanogramInfo> downloadList = serverPlanograms.Where((PlanogramInfo p) => forceDownload || !localFiles.Any((FileInfo f) => f.Name.ToUpper() == p.Filename.ToUpper()) || localFiles.Any((FileInfo f) => f.Name.ToUpper() == p.Filename.ToUpper() && f.LastWriteTimeUtc < p.LastUpdateUtc)).ToList();
			IEnumerable<FileInfo> deleteList = localFiles.Where((FileInfo f) => !serverPlanograms.Any((PlanogramInfo p) => p.Filename.ToUpper() == f.Name.ToUpper()));
			if (await DownloadPlanograms(downloadList, _planogramPath))
			{
				foreach (FileInfo item in deleteList)
				{
					_logger.LogInfoWithSource("Deleting planogram " + item.Name, "SyncFilesIfNeeded");
					DeleteFile(_planogramPath, item.Name);
				}
				syncInfo.SyncComplete = true;
				await _persistentDataCacheService.Write(syncInfo, "CurrentPlanograms");
				return;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception syncing planograms", "SyncFilesIfNeeded");
		}
		int num = _updatePeriod / 300;
		if (++retryCount >= num)
		{
			_logger.LogErrorWithSource("Sync files reached maximum retry count. Stopping the process.", "SyncFilesIfNeeded");
			return;
		}
		_logger.LogInfoWithSource($"Retrying planogram sync in {5} minutes", "SyncFilesIfNeeded");
		TimeSpan delay = TimeSpan.FromMinutes(5.0);
		Task.Run(async delegate
		{
			await Task.Delay(delay);
			await SyncFilesIfNeeded(retryCount).ConfigureAwait(continueOnCapturedContext: false);
		});
	}

	public async Task RefreshIfNeeded()
	{
		_logger?.LogInfoWithSource($"Started with random refresh delay of {_updateSeconds} seconds", "RefreshIfNeeded");
		PersistentDataWrapper<SyncPlanogramInfo> persistentDataWrapper = await _persistentDataCacheService.Read<SyncPlanogramInfo>("CurrentPlanograms", useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set - refreshing", "RefreshIfNeeded");
			await SyncPlanograms();
		}
		else if (NeedsUpdate(DateTime.Now - persistentDataWrapper.Modified))
		{
			_logger?.LogInfoWithSource($"Cache is from {persistentDataWrapper.Modified} - refreshing", "RefreshIfNeeded");
			await SyncPlanograms();
		}
	}

	private async Task<SyncPlanogramInfo> GetSyncInfo()
	{
		_logger?.LogInfoWithSource("Started", "GetSyncInfo");
		PersistentDataWrapper<SyncPlanogramInfo> persistentDataWrapper = await _persistentDataCacheService.Read<SyncPlanogramInfo>("CurrentPlanograms", useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "GetSyncInfo");
			return new SyncPlanogramInfo();
		}
		return persistentDataWrapper.Data;
	}

	private async Task<bool> DownloadPlanograms(List<PlanogramInfo> downloadList, string path)
	{
		SelectedPlanogramInfo selectedPlanogram = await GetSelectedPlanogramInfo();
		foreach (PlanogramInfo planogram in downloadList.OrderBy((PlanogramInfo d) => (!(d.Name == selectedPlanogram?.Name)) ? 1 : 0))
		{
			try
			{
				_logger.LogInfoWithSource("Downloading planogram " + planogram.Filename, "DownloadPlanograms");
				try
				{
					APIResponse<byte[]> aPIResponse = await _httpService.DownloadFile(planogram.PresignedUrl);
					if (!aPIResponse.IsSuccessStatusCode)
					{
						_logger.LogErrorWithSource($"Received status {aPIResponse.StatusCode} downloading Planogram {planogram.Filename} from {planogram.PresignedUrl}", "DownloadPlanograms");
						return false;
					}
					await File.WriteAllBytesAsync(Path.Combine(path, planogram.Filename), aPIResponse.Response);
				}
				catch (Exception ex)
				{
					_logger.LogErrorWithSource(ex, "Error writing to Planogram " + planogram.Filename, "DownloadPlanograms");
					return false;
				}
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Error opening planogram file " + planogram.Filename, "DownloadPlanograms");
				return false;
			}
		}
		return true;
	}

	private void DeleteFile(string path, string file)
	{
		try
		{
			File.Delete(Path.Combine(path, file));
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unable to delete file " + file);
		}
	}

	private void CheckPlanogramPath(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	private async Task<SelectedPlanogramInfo> GetSelectedPlanogramInfo()
	{
		SelectedPlanogramInfo result = new SelectedPlanogramInfo();
		try
		{
			string path = Path.Combine(_planogramPath, "selected_planogram.json");
			if (File.Exists(path))
			{
				using StreamReader streamReader = File.OpenText(path);
				result = JsonConvert.DeserializeObject<SelectedPlanogramInfo>(await streamReader.ReadToEndAsync());
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unable to load selected planogram.");
		}
		return result;
	}

	private async Task<SavePlanogramInfoResponse> SaveSelectedPlanogramInfo(SelectedPlanogramInfo info)
	{
		SavePlanogramInfoResponse result = new SavePlanogramInfoResponse();
		string fileContents = JsonConvert.SerializeObject(info, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		});
		try
		{
			using StreamWriter outputFile = new StreamWriter(Path.Combine(_planogramPath, "selected_planogram.json"));
			await outputFile.WriteAsync(fileContents);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "PlanogramService.SavePlanogramInfo failed to write info " + fileContents + " to " + Path.Combine(_planogramPath, "selected_planogram.json"));
			result.StatusCode = HttpStatusCode.InternalServerError;
			result.Error = ex.Message;
		}
		return result;
	}

	private bool NeedsUpdate(TimeSpan age)
	{
		DateTime? lastSync;
		lock (_syncObject)
		{
			lastSync = _lastSync;
		}
		if (!lastSync.HasValue)
		{
			return true;
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = DateTime.Today.AddHours(17.0);
		if (now > dateTime)
		{
			return false;
		}
		DateTime dateTime2 = lastSync.Value.Date.AddHours(lastSync.Value.Hour).AddSeconds(_updatePeriod + _updateSeconds);
		if (DateTime.Now >= dateTime2 && age.TotalSeconds > (double)_updatePeriod)
		{
			return true;
		}
		return false;
	}
}
