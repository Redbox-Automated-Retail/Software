using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ProductPricing.Model;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.Pricing;

public class PricingService : IPricingService
{
	private IKioskClientServicePricing _kcsPricing;

	private Store _store;

	private IBundleBasedPricingService _bundleBasedPricingService;

	private IKioskPricingService _kioskPricingService;

	private IConfiguration _configuration;

	private IPricingData _pricingData;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private Timer _changeTimer;

	private int _inRefresh;

	public bool IsPricingEngineEnabled
	{
		get
		{
			if (_kioskPricingService == null)
			{
				return false;
			}
			if (!_kioskPricingService.IsPricingEngineDataAvailable)
			{
				return false;
			}
			if (!_configuration.EnablePricingEngine)
			{
				return false;
			}
			return true;
		}
	}

	private Store Store
	{
		get
		{
			if (_store == null)
			{
				_store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
			}
			return _store;
		}
	}

	public event PricingSetChange OnPricingSetChange;

	public PricingService(IKernelService kernelService, IStoreManager storeManager)
	{
		_bundleBasedPricingService = new BundleBasedPricingService();
		_pricingData = new PricingData(kernelService, storeManager);
		_kioskPricingService = new KioskPricingService(_pricingData, storeManager.KioskId, Redbox.Rental.Services.Configuration.Configuration.Instance);
		_configuration = Redbox.Rental.Services.Configuration.Configuration.Instance;
		_kcsPricing = ServiceLocator.Instance.GetService<IKioskClientServicePricing>();
		if (_changeTimer == null)
		{
			_changeTimer = new Timer(delegate
			{
				OnTimer();
			});
		}
	}

	public void Start()
	{
		try
		{
			LogHelper.Instance.Log("PricingService.Start");
			int seconds = new Random().Next(5, 55);
			int minutes = 2;
			_changeTimer.Change(new TimeSpan(0, 1, seconds), new TimeSpan(0, minutes, seconds));
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("PricingService.Start - an exception occurred.", e);
		}
	}

	public void Stop()
	{
		try
		{
			LogHelper.Instance.Log("PricingService.Stop");
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("PricingService.Stop - an exception occurred.", e);
		}
	}

	private void OnTimer()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("PricingService.Refresh - exiting already in OnTimer");
		}
		else if (!_kcsPricing.IsKCSPricingEnabled)
		{
			try
			{
				_pricingData.LoadStaged();
			}
			finally
			{
				_inRefresh = 0;
			}
		}
	}

	public void ClearOnPricingSetChange()
	{
		OnPricingSetChange = null;
	}

	public void Reload()
	{
		LogHelper.Instance.Log("PricingService.Reload");
		try
		{
			IBundleBasedPricingService bundleBasedPricingService = _bundleBasedPricingService;
			if (bundleBasedPricingService != null && bundleBasedPricingService.Reload() && OnPricingSetChange != null)
			{
				OnPricingSetChange();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingService.Reload - Unhandled exception occurred", e);
		}
	}

	public void UpdatePricingInstance()
	{
		LogHelper.Instance.Log("PricingService.UpdatePricingInstance");
		try
		{
			if (_kcsPricing.IsKCSPricingEnabled)
			{
				_kcsPricing.CachePricing();
			}
			else if (_kioskPricingService != null)
			{
				_kioskPricingService.UpdatePricingInstance();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingService.UpdatePricingInstance - Unhandled exception occurred", e);
		}
	}

	private ITitleProduct GetTitleProduct(long productId)
	{
		if (productId == 0L)
		{
			LogHelper.Instance.LogError("PricingService", "GetTitleProduct - productID cannot be zero");
			return null;
		}
		ServiceLocator.Instance.GetService<IDataService>().GetProduct(productId, excludeTitleRollup: true, out var kioskProduct);
		if (kioskProduct == null)
		{
			LogHelper.Instance.LogError("PricingService", $"GetTitleProduct - ProductId: {productId} not found in dataservice.getproduct");
			return null;
		}
		if (!(kioskProduct is ITitleProduct result))
		{
			LogHelper.Instance.LogError("PricingService", $"GetTitleProduct - ProductId: {productId} is not a TitleProduct");
			return null;
		}
		return result;
	}

	private ProductPricing GetKCSPricing(long productId)
	{
		if (!_kcsPricing.IsKCSPricingEnabled)
		{
			return null;
		}
		ITitleProduct titleProduct = GetTitleProduct(productId);
		if (titleProduct == null)
		{
			return null;
		}
		return GetKCSPricing(titleProduct);
	}

	private ProductPricing GetKCSPricing(ITitleProduct titleProduct)
	{
		if (titleProduct == null)
		{
			LogHelper.Instance.LogError("PricingService", "GetKCSPricing - TitleProduct cannot be null");
			return null;
		}
		if (!_kcsPricing.IsKCSPricingEnabled)
		{
			return null;
		}
		ProductPricing localProductPricing = _kcsPricing.GetLocalProductPricing(titleProduct.ProductId, titleProduct.TitleType.ToPriceLevelType(), Store.RentalTaxRate, Store.PurchaseTaxRate, Store.State);
		if (localProductPricing == null || localProductPricing.RentalPriceDefault == null)
		{
			LogHelper.Instance.LogError("PricingService", $"GetKCSPricing - TitleProduct.ProductId({titleProduct.ProductId}) pricing not found");
		}
		return localProductPricing;
	}

	private ProductPricing GetKCSDefaultPricing(TitleType titleType)
	{
		PriceLevelType priceLevelType = titleType.ToPriceLevelType();
		if (priceLevelType == PriceLevelType.None)
		{
			LogHelper.Instance.LogError("PricingService", "GetKCSDefaultPricing - TitleType is none");
			return null;
		}
		if (!_kcsPricing.IsKCSPricingEnabled)
		{
			return null;
		}
		ProductPricing localProductDefaultPricing = _kcsPricing.GetLocalProductDefaultPricing(priceLevelType, Store.RentalTaxRate, Store.PurchaseTaxRate, Store.State);
		if (localProductDefaultPricing == null)
		{
			LogHelper.Instance.LogError("PricingService", $"GetKCSDefaultPricing - TitleType({titleType}) pricing not found");
		}
		return localProductDefaultPricing;
	}

	public IPricingRecord GetPurchasePricing(ITitleProduct titleProduct)
	{
		if (_kcsPricing.IsKCSPricingEnabled)
		{
			ProductPricing kCSPricing = GetKCSPricing(titleProduct);
			if (kCSPricing != null)
			{
				if (kCSPricing.PurchasePrice == null)
				{
					return null;
				}
				return kCSPricing.PurchasePrice.ToPricingRecord(titleProduct.TitleFamily, titleProduct.TitleType);
			}
		}
		else if (IsPricingEngineEnabled)
		{
			return _kioskPricingService.GetPurchasePricing(titleProduct);
		}
		return null;
	}

	public IPricingRecord GetOneDayProductPricingRecord(long productId)
	{
		IPricingRecord pricingRecord = null;
		if (_kcsPricing.IsKCSPricingEnabled)
		{
			ITitleProduct titleProduct = GetTitleProduct(productId);
			if (titleProduct == null)
			{
				return null;
			}
			ProductPricing kCSPricing = GetKCSPricing(titleProduct);
			if (kCSPricing != null && kCSPricing.RentalPrice != null)
			{
				return kCSPricing.RentalPrice.ToPricingRecord(titleProduct.TitleFamily, titleProduct.TitleType);
			}
		}
		else if (IsPricingEngineEnabled)
		{
			pricingRecord = _kioskPricingService.GetOneDayProductPricingRecord(productId);
		}
		if (pricingRecord == null)
		{
			pricingRecord = _bundleBasedPricingService.GetProductPricingRecord(productId);
		}
		return pricingRecord;
	}

	public IList<IPricingRecord> GetProductPricingRecords(long productId)
	{
		IList<IPricingRecord> list = null;
		if (_kcsPricing.IsKCSPricingEnabled)
		{
			ITitleProduct titleProduct = GetTitleProduct(productId);
			if (titleProduct == null)
			{
				return null;
			}
			ProductPricing kCSPricing = GetKCSPricing(titleProduct);
			if (kCSPricing != null && kCSPricing.RentalPrice != null)
			{
				list = new List<IPricingRecord>();
				IPricingRecord item = kCSPricing.RentalPrice.ToPricingRecord(titleProduct.TitleFamily, titleProduct.TitleType);
				list.Add(item);
				ProductRentalPrice productRentalPrice = kCSPricing.MultiNightRentalPrices.FirstOrDefault();
				if (productRentalPrice != null)
				{
					list.Add(productRentalPrice.ToPricingRecord(titleProduct.TitleFamily, titleProduct.TitleType));
				}
				return list;
			}
		}
		else if (IsPricingEngineEnabled)
		{
			list = _kioskPricingService.GetProductPricingRecords(productId);
		}
		if (list == null)
		{
			list = new List<IPricingRecord>();
			list.Add(_bundleBasedPricingService.GetProductPricingRecord(productId));
		}
		return list;
	}

	public IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool useAdjustedPrices)
	{
		IPricingRecord pricingRecord = null;
		if (_kcsPricing.IsKCSPricingEnabled)
		{
			ProductPricing kCSDefaultPricing = GetKCSDefaultPricing(titleType);
			if (kCSDefaultPricing != null && kCSDefaultPricing.RentalPriceDefault != null)
			{
				return kCSDefaultPricing.RentalPriceDefault.ToPricingRecord(titleFamily, titleType);
			}
		}
		else if (IsPricingEngineEnabled)
		{
			pricingRecord = _kioskPricingService.GetProductTypePricingRecord(titleFamily, titleType);
		}
		if (pricingRecord == null)
		{
			pricingRecord = _bundleBasedPricingService.GetProductTypePricingRecord(titleFamily, titleType, useAdjustedPrices);
		}
		return pricingRecord;
	}

	public IPricingRecord GetActiveProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType)
	{
		if (_kcsPricing.IsKCSPricingEnabled)
		{
			ProductPricing kCSDefaultPricing = GetKCSDefaultPricing(titleType);
			if (kCSDefaultPricing != null && kCSDefaultPricing.RentalPriceDefault != null)
			{
				return kCSDefaultPricing.RentalPriceDefault.ToPricingRecord(titleFamily, titleType);
			}
		}
		else if (IsPricingEngineEnabled)
		{
			return _kioskPricingService.GetActiveProductTypePricingRecord(titleFamily, titleType);
		}
		return null;
	}

	public decimal? GetPriceSetting(TitleFamily titleFamily, TitleType titleType, string settingName, RentalShoppingCartItemAction action)
	{
		decimal? result = null;
		IPricingRecord productTypePricingRecord = GetProductTypePricingRecord(titleFamily, titleType, useAdjustedPrices: false);
		if (productTypePricingRecord != null)
		{
			switch (settingName)
			{
			case "initial_night":
				result = productTypePricingRecord.InitialNight;
				break;
			case "extra_night":
				result = productTypePricingRecord.ExtraNight;
				break;
			case "expiration_price":
				result = productTypePricingRecord.ExpirationPrice;
				break;
			case "promo_value":
				result = productTypePricingRecord.PromoValue;
				break;
			}
			if (result.HasValue && result.HasValue)
			{
				result = GetAdjustedPrice(result.Value, action, titleType);
			}
		}
		return result;
	}

	public decimal GetAdjustedPrice(decimal price, RentalShoppingCartItemAction action, TitleType titleType)
	{
		decimal result = price;
		if (Store?.State == "MD")
		{
			decimal num = 0m;
			switch (action)
			{
			case RentalShoppingCartItemAction.rental:
				num = Store.RentalTaxRate;
				break;
			case RentalShoppingCartItemAction.purchase:
				num = ((titleType == TitleType.DigitalCode) ? Store.DigitalPurchaseTaxRate : Store.PurchaseTaxRate);
				break;
			}
			if (num != 0m)
			{
				result = Math.Round(price * (num / 100m + 1m), 2);
			}
		}
		return result;
	}

	public decimal GetRentalTaxRate()
	{
		decimal result = 0m;
		if (Store != null && Store.State != "MD" && Store.State != "SC")
		{
			return Store.RentalTaxRate;
		}
		return result;
	}

	public decimal GetPurchaseTaxRate()
	{
		decimal result = 0m;
		if (Store != null && Store.State != "MD" && Store.State != "SC")
		{
			return Store.PurchaseTaxRate;
		}
		return result;
	}

	public decimal GetDigitalPurchaseTaxRate()
	{
		decimal result = 0m;
		if (Store != null && Store.State != "MD" && Store.State != "SC")
		{
			return Store.DigitalPurchaseTaxRate;
		}
		return result;
	}

	public decimal GetEmptyCasePrice()
	{
		return GetPriceSetting(TitleFamily.Movies, TitleType.DVD, "initial_night", RentalShoppingCartItemAction.purchase).GetValueOrDefault();
	}
}
