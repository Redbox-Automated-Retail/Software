using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProductGroup;
using KioskClientService.API.Services.Products.ProductReelsApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public class MoreLikeThisService : IMoreLikeThisService
{
	private readonly IMoreLikeThisData _moreLikeThisData;

	private readonly IProductReelsApiClient _productReelsApiClient;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly ILogger<MoreLikeThisService> _logger;

	private IMoreLikeThisCache _moreLikeThisCache;

	public MoreLikeThisService(IMoreLikeThisData moreLikeThisData, IMoreLikeThisCache moreLikeThisCache, IProductReelsApiClient productReelsApiClient, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, ILogger<MoreLikeThisService> logger)
	{
		_moreLikeThisData = moreLikeThisData;
		_moreLikeThisCache = moreLikeThisCache;
		_productReelsApiClient = productReelsApiClient;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_logger = logger;
	}

	public GetMoreLikeThisResponse GetMLT(long productGroupId)
	{
		_logger.LogInfoWithSource($"Getting more like this from cache for {productGroupId}", "GetMLT");
		GetMoreLikeThisResponse getMoreLikeThisResponse = new GetMoreLikeThisResponse();
		getMoreLikeThisResponse.MoreLikeThis = _moreLikeThisCache.Get(productGroupId);
		if (!getMoreLikeThisResponse.MoreLikeThis.Any())
		{
			_logger.LogInfoWithSource($"productGroupId: {productGroupId} not in cached data.", "GetMLT");
			getMoreLikeThisResponse.Success = false;
			getMoreLikeThisResponse.StatusCode = 404;
		}
		else
		{
			_logger.LogInfoWithSource($"Found productGroupId: {productGroupId} in cached data, returning more like this.", "GetMLT");
		}
		return getMoreLikeThisResponse;
	}

	public async Task RefreshMoreLikeThisAsync(ScrubbedProductGroupIds productGroupIds)
	{
		productGroupIds.VerifyNoInvalidProductGroupIds();
		_logger.LogInfoWithSource($"Total received productGroupIds: {productGroupIds.Count}.", "RefreshMoreLikeThisAsync");
		if (_moreLikeThisData.FileContentExpireTime < DateTime.Now || _moreLikeThisCache.TotalCount() < 1)
		{
			_logger.LogInfoWithSource($"FileContentExpireTime was {_moreLikeThisData.FileContentExpireTime.Date}. FullRefresh time has arrived, will update whole morelikethis data file.", "RefreshMoreLikeThisAsync");
			ConcurrentDictionary<long, List<long>> concurrentDictionary = await _productReelsApiClient.MoreLikeThisBulkAsync(productGroupIds);
			if (ShouldUpdateCacheAndFile(concurrentDictionary))
			{
				_logger.LogInfoWithSource("Doing full refresh to morelikethis data.", "RefreshMoreLikeThisAsync");
				_moreLikeThisCache.RefreshCache(concurrentDictionary);
				DateTime nextFullRefreshDate = DateTime.Today.AddDays(_kioskConfiguration.KioskClientDefault.MoreLikeThisDataRefreshFrequency);
				_moreLikeThisData.RefreshAll(concurrentDictionary, nextFullRefreshDate);
				_logger.LogInfoWithSource($"Next full refresh morelikethis data will be at: {nextFullRefreshDate.Date}", "RefreshMoreLikeThisAsync");
			}
		}
		else
		{
			_logger.LogInfoWithSource("Refresh date for morelikethis data hasn't come yet. Checking if we have any missing items in the file.", "RefreshMoreLikeThisAsync");
			await HandleMissingItemsAsync(productGroupIds);
		}
	}

	private async Task HandleMissingItemsAsync(List<long> productGroupIds)
	{
		List<long> list = new List<long>();
		foreach (long productGroupId in productGroupIds)
		{
			if (!_moreLikeThisCache.ContainsItem(productGroupId))
			{
				list.Add(productGroupId);
			}
		}
		if (list.Any())
		{
			_logger.LogInfoWithSource("Found missing items from MLT data. Missing Items: " + string.Join(',', list), "HandleMissingItemsAsync");
			ConcurrentDictionary<long, List<long>> concurrentDictionary = await _productReelsApiClient.MoreLikeThisBulkAsync(list);
			if (ShouldUpdateCacheAndFile(concurrentDictionary))
			{
				_moreLikeThisCache.AddItemsToCache(concurrentDictionary);
				_moreLikeThisData.RefreshAllWithCurrentExpireTime(_moreLikeThisCache.GetAllPairs());
			}
		}
		else
		{
			_logger.LogInfoWithSource("There is no new item added to the kiosk. No need to update MoreLikeThisData or cache.", "HandleMissingItemsAsync");
		}
	}

	private bool ShouldUpdateCacheAndFile(ConcurrentDictionary<long, List<long>> receivedMltDataFromProductReelsApi)
	{
		bool result = true;
		if (!receivedMltDataFromProductReelsApi.Any())
		{
			_logger.LogInfoWithSource("Didn't get any results from productReelsApi. Will not update MoreLikeThisCache or MoreLikeThisData file.", "ShouldUpdateCacheAndFile");
			result = false;
		}
		return result;
	}
}
