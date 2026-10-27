using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Inventory.Models;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Dual;

internal class DualService : IDualService
{
	private readonly IDualCacheService _cache;

	private readonly ILogger<DualService> _logger;

	private readonly IProxyApi _api;

	private readonly IStoreService _store;

	public DualService(ILogger<DualService> logger, IProxyApi api, IStoreService store, IDualCacheService cache)
	{
		_logger = logger;
		_api = api;
		_cache = cache;
		_store = store;
	}

	public async Task<RefreshResponse> Refresh(long dualKioskId)
	{
		RefreshResponse result = new RefreshResponse();
		try
		{
			Guid guid = Guid.NewGuid();
			_logger.LogInfoWithSource($"refreshing dual kiosk inventory for partner kiosk: {dualKioskId}, messageid: {guid}", "Refresh");
			KiosksAndTitlesPayload kiosksAndTitlesPayload = new KiosksAndTitlesPayload
			{
				MessageId = guid,
				SourceKioskId = _store.KioskId
			};
			kiosksAndTitlesPayload.KioskIds.Add(dualKioskId);
			APIResponse<KioskTitlesCountResponse> aPIResponse = await _api.KioskTitlesCount(kiosksAndTitlesPayload);
			if (aPIResponse == null)
			{
				result.StatusCode = HttpStatusCode.InternalServerError;
				result.Error = "No response from KioskTitlesCount api";
			}
			else if (aPIResponse.StatusCode != HttpStatusCode.OK)
			{
				result.StatusCode = aPIResponse.StatusCode;
				Error error = aPIResponse.Errors.FirstOrDefault();
				result.Error = error.Code + ":" + error.Message;
			}
			else
			{
				KioskTitlesCountResponse._KioskTitlesCount kioskTitlesCount = aPIResponse.Response.KioskTitlesCount?.FirstOrDefault();
				if (kioskTitlesCount == null)
				{
					_logger.LogErrorWithSource("No results returned from the KioskTitlesCount call", "Refresh");
					result.StatusCode = HttpStatusCode.OK;
				}
				else if (kioskTitlesCount.TitlesCount.Any())
				{
					_logger.LogInfoWithSource($"returned {kioskTitlesCount.TitlesCount.Count} titles", "Refresh");
					await _cache.Refresh(kioskTitlesCount.TitlesCount);
				}
				else
				{
					_logger.LogErrorWithSource("No title counts returned from the KioskTitlesCount call", "Refresh");
					result.StatusCode = HttpStatusCode.InternalServerError;
					result.Error = "No title counts returned from the KioskTitlesCount call";
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception in Refresh", "Refresh");
		}
		return result;
	}

	public async Task<UpdateTitlesResponse> UpdateTitles(long dualKioskId, IEnumerable<long> titleIds)
	{
		UpdateTitlesResponse result = new UpdateTitlesResponse();
		try
		{
			Guid guid = Guid.NewGuid();
			_logger.LogInfoWithSource($"refreshing dual kiosk inventory for partner kiosk: {dualKioskId}, titles: {string.Join(',', titleIds)}, messageid: {guid}", "UpdateTitles");
			KiosksAndTitlesPayload kiosksAndTitlesPayload = new KiosksAndTitlesPayload
			{
				MessageId = guid,
				SourceKioskId = _store.KioskId
			};
			kiosksAndTitlesPayload.KioskIds.Add(dualKioskId);
			kiosksAndTitlesPayload.TitleIds.AddRange(titleIds);
			APIResponse<KioskTitlesCountResponse> aPIResponse = await _api.KioskTitlesCount(kiosksAndTitlesPayload);
			if (aPIResponse == null)
			{
				result.StatusCode = HttpStatusCode.InternalServerError;
				result.Error = "No response from KioskTitlesCount api";
			}
			else if (aPIResponse.StatusCode != HttpStatusCode.OK)
			{
				result.StatusCode = aPIResponse.StatusCode;
				Error error = aPIResponse.Errors.FirstOrDefault();
				result.Error = error.Code + ":" + error.Message;
			}
			else
			{
				KioskTitlesCountResponse._KioskTitlesCount ktc = aPIResponse.Response.KioskTitlesCount?.FirstOrDefault();
				if (ktc == null)
				{
					_logger.LogErrorWithSource("no titles returned", "UpdateTitles");
				}
				else
				{
					_logger.LogInfoWithSource($"returned {ktc.TitlesCount.Count} titles", "UpdateTitles");
					await _cache.Update(titleIds, ktc.TitlesCount);
					foreach (KeyValuePair<long, int> item in ktc.TitlesCount)
					{
						result.TitlesCount.Add(item.Key, item.Value);
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception in UpdateTitles", "UpdateTitles");
		}
		return result;
	}

	public async Task<InStockInOtherResponse> InStockInOther(IEnumerable<long> titleIds)
	{
		InStockInOtherResponse result = new InStockInOtherResponse();
		try
		{
			Guid.NewGuid();
			_logger.LogInfoWithSource("checking if following title ids are in stock in partner kiosk: " + string.Join(',', titleIds), "InStockInOther");
			IEnumerable<long> enumerable = await _cache.Existing(titleIds);
			_logger.LogInfoWithSource("the following title ids are in stock in partner kiosk: " + string.Join(',', enumerable), "InStockInOther");
			result.InStock.AddRange(enumerable);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception in InStockInOther", "InStockInOther");
		}
		return result;
	}
}
