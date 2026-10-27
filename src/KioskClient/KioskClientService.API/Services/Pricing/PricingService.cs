using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Pricing;

public class PricingService : IPricingService
{
	private IProxyApi _proxyApi;

	private ILogger<PricingService> _logger;

	private IPersistentDataCacheService _persistentDataCacheService;

	private readonly IStoreService _storeService;

	private readonly IKioskConfiguration _kioskConfiguration;

	private static DateTime? _defaultPricingNextRefresh;

	private static DateTime? _rentalPricingNextRefresh;

	private static DateTime? _purchasePricingNextRefresh;

	private static int updatePeriod = 7200;

	private static int updateSeconds = new Random().Next(0, updatePeriod - 300);

	private string _cachePricingDefaultFileName => "PricingDefault";

	private string _cachePricingRentalByPriceFileName => "PricingRentalByPrice";

	private string _cachePricingPurchaseByPriceFileName => "PricingPurchaseByPrice";

	public PricingService(IProxyApi proxyApi, ILogger<PricingService> logger, IPersistentDataCacheService persistentDataCacheService, IOptionsSnapshotKioskConfiguration kioskConfiguration, IStoreService storeService)
	{
		_proxyApi = proxyApi;
		_logger = logger;
		_persistentDataCacheService = persistentDataCacheService;
		_storeService = storeService;
		_kioskConfiguration = kioskConfiguration;
		InitializeNextRefreshValues();
	}

	public async Task RefreshIfNeeded()
	{
		_logger?.LogInfoWithSource($"Started with random refresh time of {DateTime.Today.AddHours(4.0).AddSeconds(updateSeconds)}", "RefreshIfNeeded");
		await RefreshDefaultPricingIfNeeded();
		await RefreshRentalPricingByPriceIfNeeded();
		await RefreshPurchasdePricingByPriceIfNeeded();
	}

	private bool NeedsUpdate(TimeSpan? cacheAge, DateTime? nextRefresh = null)
	{
		if (!cacheAge.HasValue)
		{
			return true;
		}
		if (cacheAge.Value.TotalHours >= 26.0)
		{
			return IsTimeForRefresh(nextRefresh);
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = DateTime.Today.AddHours(4.0);
		if (now < dateTime || now > dateTime.AddSeconds(updatePeriod))
		{
			return false;
		}
		DateTime dateTime2 = dateTime.AddSeconds(updateSeconds);
		if (now > dateTime2 && cacheAge.Value.TotalSeconds > (double)updatePeriod)
		{
			return true;
		}
		return false;
	}

	private void InitializeNextRefreshValues()
	{
		if (!_defaultPricingNextRefresh.HasValue)
		{
			int num = new Random().Next(1, Math.Max(2, _kioskConfiguration.Pricing.RefreshIntervalMinutes));
			DateTime value = DateTime.Now.AddMinutes(num);
			_defaultPricingNextRefresh = value;
			_rentalPricingNextRefresh = value;
			_purchasePricingNextRefresh = value;
		}
	}

	private DateTime GetNextRefresh()
	{
		return DateTime.Now.AddMinutes(_kioskConfiguration.Pricing.RefreshIntervalMinutes);
	}

	private bool IsTimeForRefresh(DateTime? nextRefresh)
	{
		if (nextRefresh.HasValue)
		{
			return nextRefresh <= DateTime.Now;
		}
		return true;
	}

	public async Task RefreshDefaultPricingIfNeeded()
	{
		PersistentDataWrapper<DefaultPricingData> persistentDataWrapper = await _persistentDataCacheService.Read<DefaultPricingData>(_cachePricingDefaultFileName, useCache: true);
		bool flag = persistentDataWrapper == null || persistentDataWrapper.Data == null;
		string text = (flag ? "not set" : $"from ealier than today {persistentDataWrapper.Modified}");
		TimeSpan? cacheAge = (flag ? ((TimeSpan?)null) : new TimeSpan?(DateTime.Now - persistentDataWrapper.Modified));
		if (NeedsUpdate(cacheAge, _defaultPricingNextRefresh))
		{
			_logger?.LogInfoWithSource("Cache is " + text + " - refreshing", "RefreshDefaultPricingIfNeeded");
			await RefreshDefaultPricing();
		}
	}

	public async Task RefreshRentalPricingByPriceIfNeeded()
	{
		PersistentDataWrapper<RentalPricingByPriceData> persistentDataWrapper = await _persistentDataCacheService.Read<RentalPricingByPriceData>(_cachePricingRentalByPriceFileName, useCache: true);
		bool flag = persistentDataWrapper == null || persistentDataWrapper.Data == null;
		string text = (flag ? "not set" : $"from ealier than today {persistentDataWrapper.Modified}");
		TimeSpan? cacheAge = (flag ? ((TimeSpan?)null) : new TimeSpan?(DateTime.Now - persistentDataWrapper.Modified));
		if (NeedsUpdate(cacheAge, _rentalPricingNextRefresh))
		{
			_logger?.LogInfoWithSource("Cache is " + text + " - refreshing", "RefreshRentalPricingByPriceIfNeeded");
			await RefreshRentalPricingByPrice();
		}
	}

	public async Task RefreshPurchasdePricingByPriceIfNeeded()
	{
		PersistentDataWrapper<PurchasePricingByPriceData> persistentDataWrapper = await _persistentDataCacheService.Read<PurchasePricingByPriceData>(_cachePricingPurchaseByPriceFileName, useCache: true);
		bool flag = persistentDataWrapper == null || persistentDataWrapper.Data == null;
		string text = (flag ? "not set" : $"from ealier than today {persistentDataWrapper.Modified}");
		TimeSpan? cacheAge = (flag ? ((TimeSpan?)null) : new TimeSpan?(DateTime.Now - persistentDataWrapper.Modified));
		if (NeedsUpdate(cacheAge, _purchasePricingNextRefresh))
		{
			_logger?.LogInfoWithSource("Cache is " + text + " - refreshing", "RefreshPurchasdePricingByPriceIfNeeded");
			await RefreshPurchasePricingByPrice();
		}
	}

	public async Task RefreshPricing()
	{
		_logger?.LogInfoWithSource("Started", "RefreshPricing");
		await RefreshDefaultPricing();
		await RefreshRentalPricingByPrice();
		await RefreshPurchasePricingByPrice();
	}

	public async Task RefreshDefaultPricing()
	{
		_logger?.LogInfoWithSource("Started", "RefreshDefaultPricing");
		_defaultPricingNextRefresh = GetNextRefresh();
		if (_storeService.KioskId == 0L)
		{
			_logger?.LogErrorWithSource("KioskId must be > 0", "RefreshDefaultPricing");
			return;
		}
		APIResponse<KioskRentalDefaultPricingResponse> aPIResponse = await _proxyApi.GetRentalDefaultPricing(_storeService.KioskId);
		IList<FormatPricing> list = aPIResponse?.Response?.FormatPricing;
		if (list == null || !aPIResponse.IsSuccessStatusCode)
		{
			LogResponseErrors(aPIResponse);
			_logger?.LogInfoWithSource("No pricing returned or error", "RefreshDefaultPricing");
		}
		else if (await _persistentDataCacheService.Write(new DefaultPricingData
		{
			FormatPricing = list.ToList()
		}, _cachePricingDefaultFileName))
		{
			_logger.LogInfoWithSource("Cache updated", "RefreshDefaultPricing");
		}
	}

	public async Task RefreshRentalPricingByPrice()
	{
		_logger?.LogInfoWithSource("Started", "RefreshRentalPricingByPrice");
		_rentalPricingNextRefresh = GetNextRefresh();
		if (_storeService.KioskId == 0L)
		{
			_logger?.LogErrorWithSource("KioskId must be > 0", "RefreshRentalPricingByPrice");
			return;
		}
		APIResponse<KioskRentalPricingByPriceResponse> aPIResponse = await _proxyApi.GetRentalPricingByPrice(_storeService.KioskId);
		IList<RentalPricingByPrice> list = aPIResponse?.Response?.RentalPricing;
		if (list == null || !aPIResponse.IsSuccessStatusCode)
		{
			LogResponseErrors(aPIResponse);
			_logger?.LogInfoWithSource("No pricing returned or error", "RefreshRentalPricingByPrice");
		}
		else if (await _persistentDataCacheService.Write(new RentalPricingByPriceData
		{
			RentalPricing = list.ToList()
		}, _cachePricingRentalByPriceFileName))
		{
			_logger.LogInfoWithSource("Cache updated", "RefreshRentalPricingByPrice");
		}
	}

	public async Task RefreshPurchasePricingByPrice()
	{
		_logger?.LogInfoWithSource("Started", "RefreshPurchasePricingByPrice");
		_purchasePricingNextRefresh = GetNextRefresh();
		if (_storeService.KioskId == 0L)
		{
			_logger?.LogErrorWithSource("KioskId must be > 0", "RefreshPurchasePricingByPrice");
			return;
		}
		APIResponse<KioskPurchasePricingByPriceResponse> aPIResponse = await _proxyApi.GetPurchasePricingByPrice(_storeService.KioskId);
		IList<PurchasePricingByPrice> list = aPIResponse?.Response?.PurchasePricing;
		if (list == null || !aPIResponse.IsSuccessStatusCode)
		{
			LogResponseErrors(aPIResponse);
			_logger?.LogInfoWithSource("No pricing returned or error", "RefreshPurchasePricingByPrice");
		}
		else if (await _persistentDataCacheService.Write(new PurchasePricingByPriceData
		{
			PurchasePricing = list.ToList()
		}, _cachePricingPurchaseByPriceFileName))
		{
			_logger.LogInfoWithSource("Cache updated", "RefreshPurchasePricingByPrice");
		}
	}

	public async Task<GetRentalDefaultPricingResponse> GetRentalDefaultPricing()
	{
		DefaultPricingData defaultPricingData = await GetDefaultPricingData();
		if (defaultPricingData == null)
		{
			_logger.LogError("default data is null");
			return new GetRentalDefaultPricingResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
		return new GetRentalDefaultPricingResponse
		{
			FormatPricing = defaultPricingData.FormatPricing
		};
	}

	public async Task<GetRentalPricingByPriceResponse> GetRentalPricingByPrice()
	{
		RentalPricingByPriceData rentalPricingByPriceData = await GetRentalPricingByPriceData();
		if (rentalPricingByPriceData == null)
		{
			_logger.LogError("default data is null");
			return new GetRentalPricingByPriceResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
		return new GetRentalPricingByPriceResponse
		{
			RentalPricing = rentalPricingByPriceData.RentalPricing
		};
	}

	public async Task<GetPurchasePricingByPriceResponse> GetPurchasePricingByPrice()
	{
		PurchasePricingByPriceData purchasePricingByPriceData = await GetPurchasePricingByPriceData();
		if (purchasePricingByPriceData == null)
		{
			_logger.LogError("default data is null");
			return new GetPurchasePricingByPriceResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
		return new GetPurchasePricingByPriceResponse
		{
			PurchasePricing = purchasePricingByPriceData.PurchasePricing
		};
	}

	public async Task<GetProductPricingResponse> GetProductPricing(long productId, PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state)
	{
		try
		{
			ProductPricing productPricing = await GetProductPricingData(productId, priceLevelType, rentalTaxRate, purchaseTaxRate, state);
			string text = string.Empty;
			if (productPricing == null)
			{
				text = "Product pricing data is null";
			}
			else if (productPricing.RentalPriceDefault == null || !productPricing.RentalPriceDefault.IsValid)
			{
				text = "Default Rental Price does not exist or is not valid";
			}
			if (!string.IsNullOrEmpty(text))
			{
				_logger.LogError(text);
				return new GetProductPricingResponse
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Error = text
				};
			}
			if (productPricing.RentalPrice == null || !productPricing.RentalPrice.IsValid)
			{
				productPricing.RentalPrice = productPricing.RentalPriceDefault;
				_logger.LogError("Rental Price does not exist or is not valid, setting to default");
			}
			if (productPricing.PurchasePrice != null && !productPricing.PurchasePrice.IsValid)
			{
				productPricing.PurchasePrice = null;
				_logger.LogError("Purchase price is not valid, clearing");
			}
			if (productPricing.MultiNightRentalPrices != null && productPricing.MultiNightRentalPrices.Any((ProductRentalPrice mnp) => !mnp.IsValid))
			{
				productPricing.MultiNightRentalPrices.Clear();
				_logger.LogError("MultiNight price is not valid, clearing");
			}
			return new GetProductPricingResponse
			{
				ProductPricing = productPricing
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception", "GetProductPricing");
			return new GetProductPricingResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	public async Task<GetProductPricingResponse> GetProductDefaultPricing(PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state)
	{
		try
		{
			ProductPricing productPricing = await GetProductDefaultPricingData(priceLevelType, rentalTaxRate, purchaseTaxRate, state);
			string text = string.Empty;
			if (productPricing == null)
			{
				text = "Product pricing data is null";
			}
			else if (productPricing.RentalPriceDefault == null || !productPricing.RentalPriceDefault.IsValid)
			{
				text = "Default Rental Price does not exist or is not valid";
			}
			if (!string.IsNullOrEmpty(text))
			{
				_logger.LogError(text);
				return new GetProductPricingResponse
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Error = text
				};
			}
			if (productPricing.RentalPrice == null || !productPricing.RentalPrice.IsValid)
			{
				productPricing.RentalPrice = productPricing.RentalPriceDefault;
				_logger.LogError("Rental Price does not exist or is not valid, setting to default");
			}
			if (productPricing.MultiNightRentalPrices != null && productPricing.MultiNightRentalPrices.Any((ProductRentalPrice mnp) => !mnp.IsValid))
			{
				productPricing.MultiNightRentalPrices.Clear();
				_logger.LogError("MultiNight price is not valid, clearing");
			}
			return new GetProductPricingResponse
			{
				ProductPricing = productPricing
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception", "GetProductDefaultPricing");
			return new GetProductPricingResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	public void DeleteAllPricing()
	{
		_logger?.LogInfoWithSource("Started", "DeleteAllPricing");
		DeleteRentalDefaultPricing();
		DeleteRentalPricingByPrice();
		DeletePurchasePricingByPrice();
	}

	public void DeleteRentalDefaultPricing()
	{
		_logger?.LogInfoWithSource("Deleting pricing cache", "DeleteRentalDefaultPricing");
		_persistentDataCacheService.Delete(_cachePricingDefaultFileName);
	}

	public void DeleteRentalPricingByPrice()
	{
		_logger?.LogInfoWithSource("Deleting pricing cache", "DeleteRentalPricingByPrice");
		_persistentDataCacheService.Delete(_cachePricingRentalByPriceFileName);
	}

	public void DeletePurchasePricingByPrice()
	{
		_logger?.LogInfoWithSource("Deleting purchase pricing by price cache", "DeletePurchasePricingByPrice");
		_persistentDataCacheService.Delete(_cachePricingPurchaseByPriceFileName);
	}

	private async Task<DefaultPricingData> GetDefaultPricingData()
	{
		PersistentDataWrapper<DefaultPricingData> persistentDataWrapper = await _persistentDataCacheService.Read<DefaultPricingData>(_cachePricingDefaultFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "GetDefaultPricingData");
			return null;
		}
		return persistentDataWrapper.Data;
	}

	private async Task<RentalPricingByPriceData> GetRentalPricingByPriceData()
	{
		PersistentDataWrapper<RentalPricingByPriceData> persistentDataWrapper = await _persistentDataCacheService.Read<RentalPricingByPriceData>(_cachePricingRentalByPriceFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "GetRentalPricingByPriceData");
			return null;
		}
		return persistentDataWrapper.Data;
	}

	private async Task<PurchasePricingByPriceData> GetPurchasePricingByPriceData()
	{
		PersistentDataWrapper<PurchasePricingByPriceData> persistentDataWrapper = await _persistentDataCacheService.Read<PurchasePricingByPriceData>(_cachePricingPurchaseByPriceFileName, useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "GetPurchasePricingByPriceData");
			return null;
		}
		return persistentDataWrapper.Data;
	}

	private async Task<ProductPricing> GetProductPricingData(long productId, PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state)
	{
		DefaultPricingData rentalDefault = await GetDefaultPricingData();
		if (rentalDefault == null)
		{
			return null;
		}
		RentalPricingByPriceData rental = await GetRentalPricingByPriceData();
		if (rental == null)
		{
			return null;
		}
		PurchasePricingByPriceData purchasePricingByPriceData = await GetPurchasePricingByPriceData();
		ProductPricing productPricing = new ProductPricing(productId, priceLevelType, rentalTaxRate, purchaseTaxRate, state);
		productPricing.Load(rentalDefault, rental, purchasePricingByPriceData);
		return productPricing;
	}

	private async Task<ProductPricing> GetProductDefaultPricingData(PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state)
	{
		DefaultPricingData rentalDefault = await GetDefaultPricingData();
		if (rentalDefault == null)
		{
			return null;
		}
		RentalPricingByPriceData rentalPricingByPriceData = await GetRentalPricingByPriceData();
		if (rentalPricingByPriceData == null)
		{
			return null;
		}
		ProductPricing productPricing = new ProductPricing(0L, priceLevelType, rentalTaxRate, purchaseTaxRate, state);
		productPricing.Load(rentalDefault, rentalPricingByPriceData, null);
		return productPricing;
	}

	private void LogResponseErrors<T>(APIResponse<T> response)
	{
		if (response?.Errors == null || response.Errors.Count <= 0)
		{
			return;
		}
		foreach (Error error in response.Errors)
		{
			_logger.LogErrorWithSource("Error code " + error.Code + " message " + error.Message, "LogResponseErrors");
		}
	}
}
