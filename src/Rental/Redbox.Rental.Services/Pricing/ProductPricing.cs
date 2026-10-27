using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProductPricing.Model;

namespace Redbox.Rental.Services.Pricing;

public class ProductPricing
{
	public long ProductId { get; set; }

	[JsonIgnore]
	public PriceLevelType Format { get; set; }

	[JsonIgnore]
	public decimal RentalTaxRate { get; set; }

	[JsonIgnore]
	public decimal PurchaseTaxRate { get; set; }

	[JsonIgnore]
	public string State { get; set; }

	public ProductRentalPrice RentalPriceDefault { get; set; }

	public ProductRentalPrice RentalPrice { get; set; }

	public List<ProductRentalPrice> MultiNightRentalPrices { get; set; } = new List<ProductRentalPrice>();

	public ProductPurchasePrice PurchasePrice { get; set; }

	public ProductPricing(long productId, PriceLevelType priceLevelType, decimal rentalTaxRate, decimal purchaseTaxRate, string state)
	{
		ProductId = productId;
		Format = priceLevelType;
		RentalTaxRate = rentalTaxRate;
		PurchaseTaxRate = purchaseTaxRate;
		State = state;
	}

	public void Load(List<FormatPricing> defaultPricingData, List<RentalPricingByPrice> rentalPricingByPriceData, List<PurchasePricingByPrice> purchasePricingByPriceData)
	{
		DateTime now = DateTime.Now;
		UpdateRentalPriceDefault(defaultPricingData, now);
		UpdateRentalPrice(rentalPricingByPriceData, now);
		UpdatePurchasePrice(purchasePricingByPriceData, now);
	}

	private void UpdateRentalPriceDefault(List<FormatPricing> defaultPricingData, DateTime dt)
	{
		if (defaultPricingData != null)
		{
			KCSRentalPricing kCSRentalPricing = (from f in defaultPricingData
				from p in f.Prices
				where p.StartDate <= dt && (!p.EndDate.HasValue || p.EndDate > dt) && (f.Name == Format || (f.Name == PriceLevelType.Title && Convert.ToInt32(p.PriceLevelTypeValue) == ProductId))
				orderby p.StartDate descending, p.LocationRank, p.InitialNight
				select p).FirstOrDefault();
			if (kCSRentalPricing != null)
			{
				RentalPriceDefault = AdjustRentalPrice(kCSRentalPricing.ToProductRentalPrice());
			}
		}
	}

	private void UpdateRentalPrice(List<RentalPricingByPrice> rentalPricingByPriceData, DateTime dt)
	{
		if (rentalPricingByPriceData == null)
		{
			RentalPrice = RentalPriceDefault;
			return;
		}
		List<RentalPricingByPrice> source = (from p in rentalPricingByPriceData
			from l in p.PriceLevel
			where p.StartDate <= dt && (!p.EndDate.HasValue || p.EndDate > dt) && (l.PriceLevelType == Format || (l.PriceLevelType == PriceLevelType.Title && Convert.ToInt32(l.PriceLevelTypeValue) == ProductId))
			orderby p.StartDate descending, p.LocationRank, p.InitialNight
			select p).ToList();
		RentalPricingByPrice rentalPricingByPrice = (from pInitial in source
			where pInitial.InitialDays == 1
			orderby pInitial.StartDate descending, pInitial.LocationRank, pInitial.InitialNight
			select pInitial).FirstOrDefault();
		if (rentalPricingByPrice == null)
		{
			RentalPrice = RentalPriceDefault;
		}
		else
		{
			ProductRentalPrice productRentalPrice = AdjustRentalPrice(rentalPricingByPrice.ToProductRentalPrice());
			if (productRentalPrice.IsPriceTest)
			{
				productRentalPrice.DefaultInitialNight = productRentalPrice.InitialNight;
				productRentalPrice.DefaultExtraNight = productRentalPrice.ExtraNight;
			}
			else
			{
				productRentalPrice.DefaultInitialNight = RentalPriceDefault.InitialNight;
				productRentalPrice.DefaultExtraNight = RentalPriceDefault.ExtraNight;
			}
			RentalPrice = productRentalPrice;
		}
		List<RentalPricingByPrice> list = (from pInitial in source
			where pInitial.InitialDays > 1
			orderby pInitial.StartDate descending, pInitial.LocationRank, pInitial.InitialNight
			select pInitial).ToList();
		MultiNightRentalPrices.Clear();
		list.ForEach(delegate(RentalPricingByPrice mnp)
		{
			ProductRentalPrice productRentalPrice2 = AdjustRentalPrice(mnp.ToProductRentalPrice());
			if (RentalPrice.IsPriceTest)
			{
				productRentalPrice2.DefaultInitialNight = RentalPrice.InitialNight;
				productRentalPrice2.DefaultExtraNight = RentalPrice.ExtraNight;
			}
			else
			{
				productRentalPrice2.DefaultInitialNight = RentalPriceDefault.InitialNight;
				productRentalPrice2.DefaultExtraNight = RentalPriceDefault.ExtraNight;
			}
			MultiNightRentalPrices.Add(productRentalPrice2);
		});
	}

	private void UpdatePurchasePrice(List<PurchasePricingByPrice> purchasePricingByPriceData, DateTime dt)
	{
		if (purchasePricingByPriceData != null)
		{
			PurchasePricingByPrice purchasePricingByPrice = (from p in purchasePricingByPriceData
				from t in p.Titles
				where t == ProductId && p.StartDate <= dt && (!p.EndDate.HasValue || p.EndDate > dt)
				orderby p.StartDate descending, p.PurchasePrice
				select p).FirstOrDefault();
			if (purchasePricingByPrice != null)
			{
				PurchasePrice = AdjustPurchasePrice(purchasePricingByPrice.ToProductPurchasePrice());
			}
		}
	}

	private decimal AdjustPrice(decimal price, decimal taxRate)
	{
		return Math.Round(price * (taxRate / 100m + 1m), 2);
	}

	private ProductRentalPrice AdjustRentalPrice(ProductRentalPrice rp)
	{
		if (rp == null)
		{
			return null;
		}
		if (State.ToUpper() != "MD")
		{
			return rp;
		}
		if (RentalTaxRate == 0m)
		{
			return rp;
		}
		rp.InitialNight = AdjustPrice(rp.InitialNight, RentalTaxRate);
		rp.ExtraNight = AdjustPrice(rp.ExtraNight, RentalTaxRate);
		rp.ExpirePrice = AdjustPrice(rp.ExpirePrice, RentalTaxRate);
		rp.NonReturnPrice = AdjustPrice(rp.NonReturnPrice, RentalTaxRate);
		rp.DefaultInitialNight = AdjustPrice(rp.DefaultInitialNight, RentalTaxRate);
		rp.DefaultExtraNight = AdjustPrice(rp.DefaultExtraNight, RentalTaxRate);
		return rp;
	}

	private ProductPurchasePrice AdjustPurchasePrice(ProductPurchasePrice pp)
	{
		if (State.ToUpper() != "MD")
		{
			return pp;
		}
		pp.PurchasePrice = AdjustPrice(pp.PurchasePrice, PurchaseTaxRate);
		return pp;
	}
}
