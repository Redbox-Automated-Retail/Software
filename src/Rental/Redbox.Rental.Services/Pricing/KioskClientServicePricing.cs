using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ProductPricing.Model;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Services.App;
using Redbox.Rental.Services.KioskClientService;

namespace Redbox.Rental.Services.Pricing;

public class KioskClientServicePricing : BaseKioskClientService, IKioskClientServicePricing
{
	private const string _controllerPrefix = "api/pricing";

	private IHttpService _httpService;

	private IConfiguration _configuration;

	private IStoreManager _storeManager;

	private Store _store;

	private IDataService _dataService;

	private ConcurrentDictionary<long, ProductPricing> _cache = new ConcurrentDictionary<long, ProductPricing>();

	private ConcurrentDictionary<PriceLevelType, ProductPricing> _priceLevelCache = new ConcurrentDictionary<PriceLevelType, ProductPricing>();

	private List<FormatPricing> _rentalDefaultPricing { get; set; }

	private List<RentalPricingByPrice> _rentalPricingByPrice { get; set; }

	private List<PurchasePricingByPrice> _purchasePricingByPrice { get; set; }

	public bool IsKCSPricingEnabled => _configuration.UseKCSPricing;

	public KioskClientServicePricing()
	{
		_httpService = ServiceLocator.Instance.GetService<IHttpService>();
		_configuration = ServiceLocator.Instance.GetService<IConfiguration>();
		_storeManager = ServiceLocator.Instance.GetService<IStoreManager>();
		_dataService = ServiceLocator.Instance.GetService<IDataService>();
		_store = _dataService?.GetStore();
		CachePricing();
	}

	public void CachePricing()
	{
		_rentalDefaultPricing = GetRentalDefaultPricingData();
		_rentalPricingByPrice = GetRentalPricingData();
		_purchasePricingByPrice = GetPurchasePricingData();
		_cache.Clear();
		_priceLevelCache.Clear();
	}

	private async Task<APIResponse<GetRentalDefaultPricingResponse>> GetRentalDefaultPricing()
	{
		string text = "KCS.Pricing.GetRentalDefaultPricing";
		LogHelper.Instance.Log(text ?? "");
		HttpRequestMessage request = _httpService.GenerateRequest(base.KioskClientServiceBaseUrl, "api/pricing/rentaldefault", null, HttpMethod.Get);
		return await _httpService.SendRequestAsync<GetRentalDefaultPricingResponse>(request, null, "GetRentalDefaultPricing", "c:\\build\\Redbox.Rental.Services\\Pricing\\KioskClientServicePricing.cs");
	}

	private async Task<APIResponse<GetRentalPricingByPriceResponse>> GetRentalPricing()
	{
		string text = "KCS.Pricing.GetRentalPricingByPriceResponse";
		LogHelper.Instance.Log(text ?? "");
		HttpRequestMessage request = _httpService.GenerateRequest(base.KioskClientServiceBaseUrl, "api/pricing/rental", null, HttpMethod.Get);
		return await _httpService.SendRequestAsync<GetRentalPricingByPriceResponse>(request, null, "GetRentalPricing", "c:\\build\\Redbox.Rental.Services\\Pricing\\KioskClientServicePricing.cs");
	}

	private async Task<APIResponse<GetPurchasePricingByPriceResponse>> GetPurchasePricing()
	{
		string text = "KCS.Pricing.GetPurchasePricingByPriceResponse";
		LogHelper.Instance.Log(text ?? "");
		HttpRequestMessage request = _httpService.GenerateRequest(base.KioskClientServiceBaseUrl, "api/pricing/purchase", null, HttpMethod.Get);
		return await _httpService.SendRequestAsync<GetPurchasePricingByPriceResponse>(request, null, "GetPurchasePricing", "c:\\build\\Redbox.Rental.Services\\Pricing\\KioskClientServicePricing.cs");
	}

	private async Task<APIResponse<GetProductPricingResponse>> GetProductPricing(ProductPricingRequest request)
	{
		string text = "KCS.Pricing.GetPurchasePricingByPriceResponse";
		LogHelper.Instance.Log(text ?? "");
		HttpRequestMessage request2 = _httpService.GenerateRequest(base.KioskClientServiceBaseUrl, "api/pricing/product", request, HttpMethod.Post);
		return await _httpService.SendRequestAsync<GetProductPricingResponse>(request2, null, "GetProductPricing", "c:\\build\\Redbox.Rental.Services\\Pricing\\KioskClientServicePricing.cs");
	}

	public ProductPricing GetLocalProductPricing(long productId, PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state)
	{
		try
		{
			if (!IsKCSPricingEnabled)
			{
				return null;
			}
			if (_cache.TryGetValue(productId, out var value) && value != null)
			{
				return value;
			}
			if (_rentalDefaultPricing == null)
			{
				LogHelper.Instance.LogError("KioskClientServicePricing", "GetLocalProductPricing _rentalDefaultPricing is null");
				return null;
			}
			if (_rentalPricingByPrice == null)
			{
				LogHelper.Instance.LogError("KioskClientServicePricing", "GetLocalProductPricing _rentalPricingByPrice is null");
			}
			ProductPricing productPricing = new ProductPricing(productId, priceLevelType, rentalTaxRate, purchaseTaxRate, state);
			productPricing.Load(_rentalDefaultPricing, _rentalPricingByPrice, _purchasePricingByPrice);
			_cache[productId] = productPricing;
			return productPricing;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskClientServicePricing.GetLocalProductPricing - Unhandled exception occurred", e);
			return null;
		}
	}

	public ProductPricing GetLocalProductDefaultPricing(PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state)
	{
		try
		{
			if (!IsKCSPricingEnabled)
			{
				return null;
			}
			if (_priceLevelCache.TryGetValue(priceLevelType, out var value) && value != null)
			{
				return value;
			}
			if (_rentalDefaultPricing == null)
			{
				LogHelper.Instance.LogError("KioskClientServicePricing", "GetLocalProductPricing _rentalDefaultPricing is null");
				return null;
			}
			ProductPricing productPricing = new ProductPricing(0L, priceLevelType, rentalTaxRate, purchaseTaxRate, state);
			productPricing.Load(_rentalDefaultPricing, _rentalPricingByPrice, null);
			_priceLevelCache[priceLevelType] = productPricing;
			return productPricing;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskClientServicePricing.GetLocalProductDefaultPricing - Unhandled exception occurred", e);
			return null;
		}
	}

	private List<FormatPricing> GetRentalDefaultPricingData()
	{
		string text = "KCS.Pricing.GetRentalDefaultPricingData";
		LogHelper.Instance.Log(text ?? "");
		WebResult<GetRentalDefaultPricingResponse> webResult = ExecuteWebRequest<GetRentalDefaultPricingResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/pricing/rentaldefault"), null, "GET", 20000);
		if (webResult == null || webResult.Response == null || webResult.StatusCode != HttpStatusCode.OK)
		{
			LogHelper.Instance.Log(text + " returned an error or null");
			return null;
		}
		return webResult?.Response?.FormatPricing?.ToList();
	}

	private List<RentalPricingByPrice> GetRentalPricingData()
	{
		string text = "KCS.Pricing.GetRentalPricingByPriceResponse";
		LogHelper.Instance.Log(text ?? "");
		WebResult<GetRentalPricingByPriceResponse> webResult = ExecuteWebRequest<GetRentalPricingByPriceResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/pricing/rental"), null, "GET", 20000);
		if (webResult == null || webResult.Response == null || webResult.StatusCode != HttpStatusCode.OK)
		{
			LogHelper.Instance.Log(text + " returned an error or null");
			return null;
		}
		return webResult?.Response?.RentalPricing?.ToList();
	}

	private List<PurchasePricingByPrice> GetPurchasePricingData()
	{
		string text = "KCS.Pricing.GetPurchasePricingByPriceResponse";
		LogHelper.Instance.Log(text ?? "");
		WebResult<GetPurchasePricingByPriceResponse> webResult = ExecuteWebRequest<GetPurchasePricingByPriceResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/pricing/purchase"), null, "GET", 20000);
		if (webResult == null || webResult.Response == null || webResult.StatusCode != HttpStatusCode.OK)
		{
			LogHelper.Instance.Log(text + " returned an error or null");
			return null;
		}
		return webResult?.Response?.PurchasePricing?.ToList();
	}
}
