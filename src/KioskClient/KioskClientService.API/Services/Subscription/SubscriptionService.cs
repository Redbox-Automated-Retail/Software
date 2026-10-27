using System;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Subscription;

public class SubscriptionService : ISubscriptionService
{
	private IProxyApi _proxyApi;

	private ILogger<SubscriptionService> _logger;

	private IPersistentDataCacheService _persistentDataCacheService;

	private readonly IKioskConfiguration _kioskConfiguration;

	private static int updatePeriod = 7200;

	private static int updateSeconds = new Random().Next(0, updatePeriod - 300);

	private string _cacheFileName => "SubscriptionPlans";

	public SubscriptionService(IProxyApi proxyApi, ILogger<SubscriptionService> logger, IPersistentDataCacheService persistentDataCacheService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_proxyApi = proxyApi;
		_logger = logger;
		_persistentDataCacheService = persistentDataCacheService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task<SubscriptionPlansData> GetSubscriptionPlanData()
	{
		_logger?.LogInfoWithSource("Started", "GetSubscriptionPlanData");
		PersistentDataWrapper<SubscriptionPlansData> persistentDataWrapper = await _persistentDataCacheService.Read<SubscriptionPlansData>(_cacheFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "GetSubscriptionPlanData");
			return new SubscriptionPlansData();
		}
		return persistentDataWrapper.Data;
	}

	public async Task RefreshIfNeeded()
	{
		_logger?.LogInfoWithSource($"Started with random refresh time of {DateTime.Today.AddHours(4.0).AddSeconds(updateSeconds)}", "RefreshIfNeeded");
		if (!_kioskConfiguration.KioskClientSubscriptions.SubscriptionPlansActive)
		{
			_logger?.LogInfoWithSource("SubscriptionPlanActive is false, deleting cache", "RefreshIfNeeded");
			_persistentDataCacheService.Delete(_cacheFileName);
			return;
		}
		PersistentDataWrapper<SubscriptionPlansData> persistentDataWrapper = await _persistentDataCacheService.Read<SubscriptionPlansData>(_cacheFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set - refreshing", "RefreshIfNeeded");
			await ForceRefreshSubscriptionPlan();
		}
		else if (persistentDataWrapper.Data.BrowseStarzSubscriptionPlan != _kioskConfiguration.KioskClientSubscriptions.BrowseStarzSubscriptionPlan)
		{
			_logger?.LogInfoWithSource("BrowseStarzSubscriptionPlan is different from current cache " + persistentDataWrapper.Data.BrowseStarzSubscriptionPlan + " - refreshing", "RefreshIfNeeded");
			await ForceRefreshSubscriptionPlan();
		}
		else if (persistentDataWrapper.Data.MyBagStarzSubscriptionPlan != _kioskConfiguration.KioskClientSubscriptions.MyBagStarzSubscriptionPlan)
		{
			_logger?.LogInfoWithSource("MyBagStarzSubscriptionPlan is different from current cache " + persistentDataWrapper.Data.MyBagStarzSubscriptionPlan + " - refreshing", "RefreshIfNeeded");
			await ForceRefreshSubscriptionPlan();
		}
		else if (NeedsUpdate(DateTime.Now - persistentDataWrapper.Modified))
		{
			_logger?.LogInfoWithSource($"Cache is from ealier than today {persistentDataWrapper.Modified} - refreshing", "RefreshIfNeeded");
			await ForceRefreshSubscriptionPlan();
		}
	}

	private bool NeedsUpdate(TimeSpan age)
	{
		if (age.TotalHours >= 26.0)
		{
			return true;
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = DateTime.Today.AddHours(4.0);
		if (now < dateTime || now > dateTime.AddSeconds(updatePeriod))
		{
			return false;
		}
		DateTime dateTime2 = dateTime.AddSeconds(updateSeconds);
		if (now > dateTime2 && age.TotalSeconds > (double)updatePeriod)
		{
			return true;
		}
		return false;
	}

	public async Task RefreshSubscriptionPlans()
	{
		await ForceRefreshSubscriptionPlan();
	}

	public void DeleteSubscriptionPlans()
	{
		_logger?.LogInfoWithSource("Deleting subscription plan cache", "DeleteSubscriptionPlans");
		_persistentDataCacheService.Delete(_cacheFileName);
	}

	private async Task ForceRefreshSubscriptionPlan()
	{
		_logger?.LogInfoWithSource("Started", "ForceRefreshSubscriptionPlan");
		if (!_kioskConfiguration.KioskClientSubscriptions.SubscriptionPlansActive)
		{
			_logger?.LogInfoWithSource("ProducutGroupSortOrderType is 0, deleting cache", "ForceRefreshSubscriptionPlan");
			_persistentDataCacheService.Delete(_cacheFileName);
			return;
		}
		SubscriptionPlansData subscriptionPlansData = await GetProxySubscriptionPlans();
		if (subscriptionPlansData == null)
		{
			_logger?.LogInfoWithSource("No subscription plan and/or Redbox+ promo campaign ids were returned", "ForceRefreshSubscriptionPlan");
		}
		else
		{
			await _persistentDataCacheService.Write(subscriptionPlansData, _cacheFileName);
		}
	}

	private async Task<SubscriptionPlansData> GetProxySubscriptionPlans()
	{
		SubscriptionPlansData result = new SubscriptionPlansData();
		APIResponse<GetProxySubscriptionPlanResponse> response = await _proxyApi.GetSubscriptionPlans();
		if (response != null && response.IsSuccessStatusCode)
		{
			result.CopyFrom(response.Response);
			result.Active = _kioskConfiguration.KioskClientSubscriptions.SubscriptionPlansActive;
			result.BrowseStarzSubscriptionPlan = _kioskConfiguration.KioskClientSubscriptions.BrowseStarzSubscriptionPlan;
			result.MyBagStarzSubscriptionPlan = _kioskConfiguration.KioskClientSubscriptions.MyBagStarzSubscriptionPlan;
			APIResponse<GetProxyRedboxPlusPromoCampaignsResponse> aPIResponse = await _proxyApi.GetRedboxPlusPromoCampaigns();
			if (aPIResponse != null && response.IsSuccessStatusCode)
			{
				result.CopyFrom(aPIResponse.Response);
			}
			else
			{
				_logger.LogErrorWithSource("GetRedboxPlusPromoCampaigns was unsuccessful.", "GetProxySubscriptionPlans");
				result = null;
			}
		}
		else
		{
			_logger.LogErrorWithSource(response.Exception, "GetSubscriptionPlans was unsuccessful.", "GetProxySubscriptionPlans");
			LogResponseErrors(response);
			result = null;
		}
		return result;
	}

	private void LogResponseErrors<T>(APIResponse<T> response)
	{
		if (response.Errors == null || response.Errors.Count <= 0)
		{
			return;
		}
		foreach (Error error in response.Errors)
		{
			_logger.LogErrorWithSource("Error code " + error.Code + " message " + error.Message, "LogResponseErrors");
		}
	}
}
