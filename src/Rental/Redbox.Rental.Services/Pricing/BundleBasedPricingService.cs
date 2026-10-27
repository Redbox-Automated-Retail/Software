using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Pricing;

public class BundleBasedPricingService : IBundleBasedPricingService
{
	private Store _store;

	private IPricingSet _currentPricingSet;

	private IPricingSet _bundleDefaultPricingSet = new PricingSet
	{
		PriceSetId = "1",
		PriceRecords = new List<IPricingRecord>
		{
			new PricingRecord
			{
				TitleFamily = TitleFamily.Movies,
				TitleType = TitleType.DVD,
				InitialNight = 2.25m,
				ExtraNight = 2.25m,
				ExpirationPrice = 2.25m,
				NonReturn = 36.00m,
				NonReturnDays = 16m,
				PromoValue = 2.25m,
				PriceSetId = 1
			},
			new PricingRecord
			{
				TitleFamily = TitleFamily.Movies,
				TitleType = TitleType.Bluray,
				InitialNight = 2.25m,
				ExtraNight = 2.25m,
				ExpirationPrice = 2.25m,
				NonReturn = 36.00m,
				NonReturnDays = 16m,
				PromoValue = 2.25m,
				PriceSetId = 1
			},
			new PricingRecord
			{
				TitleFamily = TitleFamily.Games,
				TitleType = TitleType.All,
				InitialNight = 3.00m,
				ExtraNight = 3.00m,
				ExpirationPrice = 3.00m,
				NonReturn = 66.00m,
				NonReturnDays = 22m,
				PromoValue = 3.00m,
				PriceSetId = 1
			},
			new PricingRecord
			{
				TitleFamily = TitleFamily.Movies,
				TitleType = TitleType._4KUHD,
				InitialNight = 2.50m,
				ExtraNight = 2.50m,
				ExpirationPrice = 2.50m,
				NonReturn = 40.00m,
				NonReturnDays = 16m,
				PromoValue = 2.50m,
				PriceSetId = 1
			}
		}
	};

	private IPricingSet _bundleDefaultSCPricingSet = new PricingSet
	{
		PriceSetId = "410",
		PriceRecords = new List<IPricingRecord>
		{
			new PricingRecord
			{
				TitleFamily = TitleFamily.Movies,
				TitleType = TitleType.DVD,
				InitialNight = 2.36m,
				ExtraNight = 2.36m,
				ExpirationPrice = 2.36m,
				NonReturn = 37.76m,
				NonReturnDays = 16m,
				PromoValue = 2.36m,
				PriceSetId = 410
			},
			new PricingRecord
			{
				TitleFamily = TitleFamily.Movies,
				TitleType = TitleType.Bluray,
				InitialNight = 2.36m,
				ExtraNight = 2.36m,
				ExpirationPrice = 2.36m,
				NonReturn = 37.76m,
				NonReturnDays = 16m,
				PromoValue = 2.36m,
				PriceSetId = 410
			},
			new PricingRecord
			{
				TitleFamily = TitleFamily.Games,
				TitleType = TitleType.All,
				InitialNight = 3.20m,
				ExtraNight = 3.20m,
				ExpirationPrice = 3.20m,
				NonReturn = 70.40m,
				NonReturnDays = 22m,
				PromoValue = 3.20m,
				PriceSetId = 410
			},
			new PricingRecord
			{
				TitleFamily = TitleFamily.Movies,
				TitleType = TitleType._4KUHD,
				InitialNight = 2.70m,
				ExtraNight = 2.70m,
				ExpirationPrice = 2.70m,
				NonReturn = 43.20m,
				NonReturnDays = 16m,
				PromoValue = 2.70m,
				PriceSetId = 410
			}
		}
	};

	public IPricingSet CurrentPriceSet => _currentPricingSet;

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

	public bool Reload()
	{
		LogHelper.Instance.Log(">  Pricing Service Load...");
		IPricingSet bundleDefaultPricingSet = GetBundleDefaultPricingSet();
		bool result = false;
		if (_currentPricingSet != null)
		{
			LogHelper.Instance.Log(">  Checking for price changes...");
			result = !bundleDefaultPricingSet.IsEqual(_currentPricingSet);
		}
		_currentPricingSet = bundleDefaultPricingSet;
		return result;
	}

	public IPricingRecord GetProductPricingRecord(long productId)
	{
		IPricingRecord result = null;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (productId != 0L)
		{
			service.GetProduct(productId, excludeTitleRollup: true, out var kioskProduct);
			if (kioskProduct != null)
			{
				if (kioskProduct is ITitleProduct titleProduct)
				{
					result = GetProductTypePricingRecord(titleProduct.TitleFamily, titleProduct.TitleType, useAdjustedPrices: true);
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in BundleBasedPricingService.GetProductPricingRecord.  Unable to find product with id {0}", productId);
			}
		}
		return result;
	}

	public IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool useAdjustedPrices)
	{
		IPricingRecord pricingRecord = null;
		IPricingRecord pricingRecord2 = CurrentPriceSet?.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == titleFamily && (x.TitleType == titleType || x.TitleType == TitleType.All));
		if (pricingRecord2 != null)
		{
			pricingRecord = pricingRecord2.Clone();
			if (useAdjustedPrices)
			{
				pricingRecord.InitialNight = GetAdjustedPrice(pricingRecord.InitialNight, RentalShoppingCartItemAction.rental, titleType);
				pricingRecord.ExtraNight = GetAdjustedPrice(pricingRecord.ExtraNight, RentalShoppingCartItemAction.rental, titleType);
				pricingRecord.ExpirationPrice = GetAdjustedPrice(pricingRecord.ExpirationPrice, RentalShoppingCartItemAction.rental, titleType);
			}
		}
		return pricingRecord;
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

	private IPricingSet GetBundleDefaultPricingSet()
	{
		IPricingSet obj = ((Store?.State == "SC") ? _bundleDefaultSCPricingSet : _bundleDefaultPricingSet);
		obj.PriceRecords.ForEach(delegate(IPricingRecord pricingRecord)
		{
			pricingRecord.NonReturn = ((Store?.State == "MD") ? (Math.Round(pricingRecord.InitialNight * (Store.RentalTaxRate / 100m + 1m), 2) * pricingRecord.NonReturnDays) : (pricingRecord.InitialNight * pricingRecord.NonReturnDays));
		});
		return obj;
	}
}
