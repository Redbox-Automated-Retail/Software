using ProductPricing.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Services.Pricing;

public static class PriceExtensions
{
	public static PriceLevelType ToPriceLevelType(this TitleType titleType)
	{
		switch (titleType)
		{
		case TitleType.DVD:
			return PriceLevelType.Dvd;
		case TitleType.Bluray:
			return PriceLevelType.Bluray;
		case TitleType._4KUHD:
			return PriceLevelType.FourKUHD;
		default:
			if (titleType.GetTitleFamily() == TitleFamily.Games)
			{
				return PriceLevelType.Games;
			}
			return PriceLevelType.None;
		}
	}

	public static IPricingRecord ToPricingRecord(this ProductPurchasePrice price, TitleFamily titleFamily, TitleType titleType)
	{
		return new PricingRecord
		{
			Purchase = price.PurchasePrice,
			TitleFamily = titleFamily,
			TitleType = titleType,
			ProductPricingId = 1L
		};
	}

	public static IPricingRecord ToPricingRecord(this ProductRentalPrice price, TitleFamily titleFamily, TitleType titleType)
	{
		return new PricingRecord
		{
			InitialDays = price.InitialDays,
			InitialNight = price.InitialNight,
			ExtraNight = price.ExtraNight,
			DefaultInitialNightPrice = price.DefaultInitialNight,
			DefaultExtraNightPrice = price.DefaultExtraNight,
			NonReturnDays = price.NonReturnDays,
			ExpirationPrice = price.ExpirePrice,
			NonReturn = price.NonReturnPrice,
			PromoValue = price.InitialNight,
			Purchase = 0m,
			TitleFamily = titleFamily,
			TitleType = titleType,
			ProductPricingId = 1L
		};
	}
}
