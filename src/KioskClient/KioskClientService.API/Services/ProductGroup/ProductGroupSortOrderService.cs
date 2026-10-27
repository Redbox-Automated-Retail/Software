using System;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Inventory.API;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.ProductGroup;

public class ProductGroupSortOrderService : IProductGroupSortOrderService
{
	private IInventoryApiClient _inventoryClient;

	private ILogger<ProductGroupSortOrderService> _logger;

	private IPersistentDataCacheService _persistentDataCacheService;

	private IKioskConfiguration _kioskConfiguration;

	private string _cacheFileName => "ProductGroupSortOrder";

	private int _productGroupSortOrderType => _kioskConfiguration.KioskClientDefault.ProductGroupSortOrderType;

	public ProductGroupSortOrderService(IInventoryApiClient inventoryApiClient, ILogger<ProductGroupSortOrderService> logger, IPersistentDataCacheService persistentDataCacheService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_inventoryClient = inventoryApiClient;
		_logger = logger;
		_persistentDataCacheService = persistentDataCacheService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task<ProductGroupSortOrder> GetSortOrder()
	{
		PersistentDataWrapper<ProductGroupSortOrder> persistentDataWrapper = await _persistentDataCacheService.Read<ProductGroupSortOrder>(_cacheFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "GetSortOrder");
			return new ProductGroupSortOrder();
		}
		return persistentDataWrapper.Data;
	}

	public async Task RefreshIfNeeded()
	{
		_logger?.LogInfoWithSource("Started", "RefreshIfNeeded");
		if (_productGroupSortOrderType == 0)
		{
			_logger?.LogInfoWithSource("ProducutGroupSortOrderType is 0, deleting cache", "RefreshIfNeeded");
			_persistentDataCacheService.Delete(_cacheFileName);
			return;
		}
		PersistentDataWrapper<ProductGroupSortOrder> persistentDataWrapper = await _persistentDataCacheService.Read<ProductGroupSortOrder>(_cacheFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set - refreshing", "RefreshIfNeeded");
			await ForceRefreshSortOrder();
		}
		else if (persistentDataWrapper.Data.SortOrderType != _productGroupSortOrderType)
		{
			_logger?.LogInfoWithSource($"SortOrderType is different from current cache {persistentDataWrapper.Data.SortOrderType} - refreshing", "RefreshIfNeeded");
			await ForceRefreshSortOrder();
		}
		else if (persistentDataWrapper.Modified < DateTime.Today)
		{
			_logger?.LogInfoWithSource($"Cache is from ealier than today {persistentDataWrapper.Modified} - refreshing", "RefreshIfNeeded");
			await ForceRefreshSortOrder();
		}
	}

	public async Task RefreshSortOrder()
	{
		await ForceRefreshSortOrder();
	}

	public void DeleteSortOrder()
	{
		_logger?.LogInfoWithSource("Deleting cache", "DeleteSortOrder");
		_persistentDataCacheService.Delete(_cacheFileName);
	}

	private async Task ForceRefreshSortOrder()
	{
		_logger?.LogInfoWithSource("Started", "ForceRefreshSortOrder");
		if (_productGroupSortOrderType == 0)
		{
			_logger?.LogInfoWithSource("ProducutGroupSortOrderType is 0, deleting cache", "ForceRefreshSortOrder");
			_persistentDataCacheService.Delete(_cacheFileName);
			return;
		}
		ProductGroupSortOrder productGroupSortOrder = await GetProductGroupSortOrder();
		if (productGroupSortOrder == null)
		{
			_logger?.LogInfoWithSource("No sort order was returned", "ForceRefreshSortOrder");
		}
		else
		{
			await _persistentDataCacheService.Write(productGroupSortOrder, _cacheFileName);
		}
	}

	private async Task<ProductGroupSortOrder> GetProductGroupSortOrder()
	{
		ProductGroupSortOrder result = new ProductGroupSortOrder();
		APIResponse<GetProductGroupSortOrder> aPIResponse = await _inventoryClient.GetProductGroupOrder(_productGroupSortOrderType);
		if (aPIResponse != null && aPIResponse.IsSuccessStatusCode && aPIResponse.Response.ProductGroupSortOrders.Count > 0)
		{
			result.CopyFrom(aPIResponse.Response);
		}
		else
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "GetProductGroupOrder was unsuccessful.", "GetProductGroupSortOrder");
			LogResponseErrors(aPIResponse);
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
