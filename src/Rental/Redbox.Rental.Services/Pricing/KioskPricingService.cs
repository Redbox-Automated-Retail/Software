using System;
using System.Collections.Generic;
using System.Linq;
using ProductPricing.Model;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.Pricing;

public class KioskPricingService : IKioskPricingService
{
	private IPricingData _pricingData;

	private IPricingDataInstance _pricingDataInstance;

	private long _kioskId;

	private KioskPricing _kioskPricing;

	private DateTime _pricingDateTime = DateTime.Now;

	private Store _store;

	private IConfiguration _configuration;

	public bool IsPricingEngineDataAvailable
	{
		get
		{
			if (_kioskPricing == null)
			{
				return false;
			}
			if (_pricingDataInstance == null)
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
				IDataService service = ServiceLocator.Instance.GetService<IDataService>();
				_store = service.GetStore();
			}
			return _store;
		}
	}

	public KioskPricingService(IPricingData pricingData, long kioskId, IConfiguration configuration)
	{
		_pricingData = pricingData;
		_kioskId = kioskId;
		_configuration = configuration;
		UpdatePricingInstance();
	}

	public void UpdatePricingInstance(bool force = false)
	{
		_pricingDataInstance = _pricingData.GetActiveInstance(force);
		if (_pricingDataInstance == null)
		{
			LogHelper.Instance.Log("KioskPricingService.UpdatePricingInstance - no instance exists", LogEntryType.Error);
		}
		else if (_pricingDataInstance.KioskPricing.KioskPricing.ContainsKey(_kioskId))
		{
			_kioskPricing = _pricingDataInstance.KioskPricing.KioskPricing[_kioskId];
			_pricingDateTime = DateTime.Now;
			LogHelper.Instance.Log($"KioskPricingService.UpdatePricingInstance - found pricing data for this kiosk - using time {_pricingDateTime}");
		}
		else
		{
			LogHelper.Instance.Log("KioskPricingService.UpdatePricingInstance - pricing file doesn't contain data for this kiosk", LogEntryType.Error);
			_kioskPricing = null;
		}
	}

	public IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType)
	{
		IPricingRecord result = null;
		if (!IsPricingEngineDataAvailable)
		{
			return null;
		}
		if (titleFamily == TitleFamily.All)
		{
			return null;
		}
		DefaultRentalPricingDTO defaultRentalPricingDTO = null;
		switch (titleFamily)
		{
		case TitleFamily.Movies:
			if (titleType == TitleType.DVD)
			{
				defaultRentalPricingDTO = GetActiveServiceDefaultRentalPricing(_pricingDataInstance.DefaultRentalPricing, _pricingDateTime, 0L, PriceLevelType.Dvd);
			}
			if (titleType == TitleType.Bluray)
			{
				defaultRentalPricingDTO = GetActiveServiceDefaultRentalPricing(_pricingDataInstance.DefaultRentalPricing, _pricingDateTime, 0L, PriceLevelType.Bluray);
			}
			if (titleType == TitleType._4KUHD)
			{
				defaultRentalPricingDTO = GetActiveServiceDefaultRentalPricing(_pricingDataInstance.DefaultRentalPricing, _pricingDateTime, 0L, PriceLevelType.FourKUHD);
			}
			break;
		case TitleFamily.Games:
			if (titleType == TitleType.All)
			{
				defaultRentalPricingDTO = GetActiveServiceDefaultRentalPricing(_pricingDataInstance.DefaultRentalPricing, _pricingDateTime, 0L, PriceLevelType.Games);
			}
			break;
		}
		if (defaultRentalPricingDTO != null)
		{
			return defaultRentalPricingDTO.ToPricingRecord(titleFamily, titleType);
		}
		return result;
	}

	public IPricingRecord GetActiveProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType)
	{
		if (!IsPricingEngineDataAvailable)
		{
			return null;
		}
		PriceLevelType priceLevel = GetPriceLevel(titleFamily, titleType);
		DefaultRentalPricingDTO activeServiceDefaultRentalPricing = GetActiveServiceDefaultRentalPricing(_pricingDataInstance.DefaultRentalPricing, _pricingDateTime, 0L, priceLevel);
		RentalPricingDTO activeServiceRentalPricing = GetActiveServiceRentalPricing(_pricingDataInstance.RentalPricing, _pricingDateTime, 0L, priceLevel);
		if (activeServiceDefaultRentalPricing == null)
		{
			return null;
		}
		if (activeServiceRentalPricing == null)
		{
			return null;
		}
		if (activeServiceRentalPricing.HasAttribute(PricingAttributes.PriceTest))
		{
			activeServiceRentalPricing.DefaultInitialNight = activeServiceRentalPricing.InitialNight;
			activeServiceRentalPricing.DefaultExtraNight = activeServiceRentalPricing.ExtraNight;
		}
		else
		{
			activeServiceRentalPricing.DefaultInitialNight = activeServiceDefaultRentalPricing.InitialNight;
			activeServiceRentalPricing.DefaultExtraNight = activeServiceDefaultRentalPricing.ExtraNight;
		}
		return activeServiceRentalPricing.ToPricingRecord(titleFamily, titleType);
	}

	public IPricingRecord GetOneDayProductPricingRecord(long productId)
	{
		if (!IsPricingEngineDataAvailable)
		{
			return null;
		}
		return GetProductPricingRecords(productId)?.Where((IPricingRecord x) => x.InitialDays == 1).FirstOrDefault();
	}

	public List<IPricingRecord> GetProductPricingRecords(long productId)
	{
		if (!IsPricingEngineDataAvailable)
		{
			return null;
		}
		if (productId == 0L)
		{
			return null;
		}
		ServiceLocator.Instance.GetService<IDataService>().GetProduct(productId, excludeTitleRollup: true, out var kioskProduct);
		if (kioskProduct == null)
		{
			LogHelper.Instance.Log($"Error in KioskPricingService.GetProductPricingRecords  Unable to find product with id {productId}");
			return null;
		}
		if (!(kioskProduct is ITitleProduct titleProduct))
		{
			LogHelper.Instance.Log($"Error in KioskPricingService.GetProductPricingRecords  KioskProduct is not a TitleProduct with id {productId}");
			return null;
		}
		List<IPricingRecord> productPricingRecordsByProductId = GetProductPricingRecordsByProductId(productId, titleProduct.TitleFamily, titleProduct.TitleType);
		IPricingRecord productTypePricingRecord = GetProductTypePricingRecord(titleProduct.TitleFamily, titleProduct.TitleType);
		List<IPricingRecord> initial = new List<IPricingRecord>();
		List<IPricingRecord> mnp = new List<IPricingRecord>();
		if (productPricingRecordsByProductId != null)
		{
			productPricingRecordsByProductId.Where((IPricingRecord x) => x.InitialDays == 1).ForEach(delegate(IPricingRecord x)
			{
				initial.Add(x);
			});
			productPricingRecordsByProductId.Where((IPricingRecord x) => x.InitialDays > 1).ForEach(delegate(IPricingRecord x)
			{
				mnp.Add(x);
			});
		}
		if (initial.Count == 0 && productTypePricingRecord != null)
		{
			initial.Add(productTypePricingRecord);
		}
		List<IPricingRecord> list = new List<IPricingRecord>();
		list.AddRange(initial);
		list.AddRange(mnp);
		return list;
	}

	private PriceLevelType GetPriceLevel(TitleFamily titleFamily, TitleType titleType)
	{
		switch (titleFamily)
		{
		case TitleFamily.Movies:
			switch (titleType)
			{
			case TitleType.DVD:
				return PriceLevelType.Dvd;
			case TitleType.Bluray:
				return PriceLevelType.Bluray;
			case TitleType._4KUHD:
				return PriceLevelType.FourKUHD;
			}
			break;
		case TitleFamily.Games:
			return PriceLevelType.Games;
		}
		return PriceLevelType.None;
	}

	private List<IPricingRecord> GetProductPricingRecordsByProductId(long productId, TitleFamily titleFamily, TitleType titleType)
	{
		if (_kioskPricing == null)
		{
			return null;
		}
		if (_pricingDataInstance == null)
		{
			return null;
		}
		PriceLevelType priceLevel = GetPriceLevel(titleFamily, titleType);
		DefaultRentalPricingDTO drp = GetActiveServiceDefaultRentalPricing(_pricingDataInstance.DefaultRentalPricing, _pricingDateTime, productId, priceLevel);
		RentalPricingDTO rp = GetActiveServiceRentalPricing(_pricingDataInstance.RentalPricing, _pricingDateTime, productId, priceLevel);
		List<RentalPricingDTO> activeMNPServiceRentalPricing = GetActiveMNPServiceRentalPricing(_pricingDataInstance.RentalPricing, _pricingDateTime, productId, priceLevel);
		if (drp == null)
		{
			return null;
		}
		if (rp == null)
		{
			rp = RentalPricingDTO.CreateFromDefaultRentalPricing(drp);
		}
		else if (rp.HasAttribute(PricingAttributes.PriceTest))
		{
			rp.DefaultInitialNight = rp.InitialNight;
			rp.DefaultExtraNight = rp.ExtraNight;
		}
		else
		{
			rp.DefaultInitialNight = drp.InitialNight;
			rp.DefaultExtraNight = drp.ExtraNight;
		}
		if (activeMNPServiceRentalPricing != null && activeMNPServiceRentalPricing.Count > 0)
		{
			activeMNPServiceRentalPricing.ForEach(delegate(RentalPricingDTO x)
			{
				if (rp.HasAttribute(PricingAttributes.PriceTest))
				{
					x.DefaultInitialNight = rp.InitialNight;
					x.DefaultExtraNight = rp.ExtraNight;
				}
				else
				{
					x.DefaultInitialNight = drp.InitialNight;
					x.DefaultExtraNight = drp.ExtraNight;
				}
			});
		}
		List<IPricingRecord> list = new List<IPricingRecord>();
		list.Add(rp.ToPricingRecord(TitleFamily.All, TitleType.All));
		if (activeMNPServiceRentalPricing != null && activeMNPServiceRentalPricing.Count > 0)
		{
			list.Add(activeMNPServiceRentalPricing[0].ToPricingRecord(TitleFamily.All, TitleType.All));
		}
		list?.Sort((IPricingRecord x, IPricingRecord y) => (x?.InitialDays ?? 1).CompareTo(y?.InitialDays ?? 1));
		return list;
	}

	public IPricingRecord GetPurchasePricing(ITitleProduct titleProduct)
	{
		if (!IsPricingEngineDataAvailable)
		{
			return null;
		}
		PurchasePricingDTO activeServicePurchasePricing = GetActiveServicePurchasePricing(_pricingDataInstance.PurchasePricing, _pricingDateTime, titleProduct.ProductId, titleProduct.TitleType == TitleType.DigitalCode);
		if (activeServicePurchasePricing == null || activeServicePurchasePricing.PurchasePrice <= 0m)
		{
			return null;
		}
		return activeServicePurchasePricing.ToPricingRecord(titleProduct.TitleFamily, titleProduct.TitleType);
	}

	private DefaultRentalPricingDTO GetActiveServiceDefaultRentalPricing(List<DefaultRentalPricing> drpList, DateTime dt, long titleId, PriceLevelType plt)
	{
		DefaultRentalPricing activeDefaultRentalPricing = drpList.GetActiveDefaultRentalPricing(dt, titleId, plt);
		if (activeDefaultRentalPricing == null)
		{
			return null;
		}
		return AdjustServiceDefaultRentalPrice(DefaultRentalPricingDTO.FromDefaultRentalPricing(activeDefaultRentalPricing));
	}

	private RentalPricingDTO GetActiveServiceRentalPricing(List<RentalPricing> rpList, DateTime dt, long titleId, PriceLevelType plt)
	{
		RentalPricing activeRentalPricing = rpList.GetActiveRentalPricing(dt, titleId, plt);
		if (activeRentalPricing == null)
		{
			return null;
		}
		return AdjustServiceRentalPrice(RentalPricingDTO.FromRentalPricing(activeRentalPricing));
	}

	private List<RentalPricingDTO> GetActiveMNPServiceRentalPricing(List<RentalPricing> rpList, DateTime dt, long titleId, PriceLevelType plt)
	{
		List<RentalPricing> activeMNPRentalPricing = rpList.GetActiveMNPRentalPricing(dt, titleId, plt);
		List<RentalPricingDTO> list = new List<RentalPricingDTO>();
		activeMNPRentalPricing.ForEach(delegate(RentalPricing x)
		{
			list.Add(AdjustServiceRentalPrice(RentalPricingDTO.FromRentalPricing(x)));
		});
		return list;
	}

	private PurchasePricingDTO GetActiveServicePurchasePricing(List<PurchasePricing> ppList, DateTime dt, long titleId, bool isDigitalPurchase)
	{
		PurchasePricing activePurchasePricing = ppList.GetActivePurchasePricing(dt, titleId, _configuration.UseLowestPriceSortOrder);
		if (activePurchasePricing == null)
		{
			return null;
		}
		return AdjustServicePurchasePrice(PurchasePricingDTO.FromPurchasePricing(activePurchasePricing), isDigitalPurchase);
	}

	private decimal AdjustPrice(decimal price, decimal taxRate)
	{
		return Math.Round(price * (taxRate / 100m + 1m), 2);
	}

	private RentalPricingDTO AdjustServiceRentalPrice(RentalPricingDTO rp)
	{
		if (_store.State.ToUpper() != "MD")
		{
			return rp;
		}
		if (Store.RentalTaxRate == 0m)
		{
			return rp;
		}
		rp.InitialNight = AdjustPrice(rp.InitialNight, Store.RentalTaxRate);
		rp.ExtraNight = AdjustPrice(rp.ExtraNight, Store.RentalTaxRate);
		rp.ExpirePrice = AdjustPrice(rp.ExpirePrice, Store.RentalTaxRate);
		rp.NonReturnPrice = AdjustPrice(rp.NonReturnPrice, Store.RentalTaxRate);
		return rp;
	}

	private PurchasePricingDTO AdjustServicePurchasePrice(PurchasePricingDTO pp, bool isDigitalPurchase)
	{
		if (Store.State.ToUpper() != "MD")
		{
			return pp;
		}
		decimal num = (isDigitalPurchase ? Store.DigitalPurchaseTaxRate : Store.PurchaseTaxRate);
		if (num == 0m)
		{
			return pp;
		}
		pp.PurchasePrice = AdjustPrice(pp.PurchasePrice, num);
		return pp;
	}

	private DefaultRentalPricingDTO AdjustServiceDefaultRentalPrice(DefaultRentalPricingDTO drp)
	{
		if (Store.State.ToUpper() != "MD")
		{
			return drp;
		}
		if (Store.RentalTaxRate == 0m)
		{
			return drp;
		}
		drp.InitialNight = AdjustPrice(drp.InitialNight, Store.RentalTaxRate);
		drp.ExtraNight = AdjustPrice(drp.ExtraNight, Store.RentalTaxRate);
		drp.ExpirePrice = AdjustPrice(drp.ExpirePrice, Store.RentalTaxRate);
		drp.NonReturnPrice = AdjustPrice(drp.NonReturnPrice, Store.RentalTaxRate);
		return drp;
	}
}
