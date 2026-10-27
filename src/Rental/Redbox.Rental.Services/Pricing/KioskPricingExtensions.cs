using ProductPricing.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Services.Pricing;

internal static class KioskPricingExtensions
{
	public static IPricingRecord ToPricingRecord(this DefaultRentalPricingDTO dto, TitleFamily titleFamily, TitleType titleType)
	{
		return new PricingRecord
		{
			InitialDays = 1,
			InitialNight = dto.InitialNight,
			ExtraNight = dto.ExtraNight,
			DefaultInitialNightPrice = dto.InitialNight,
			DefaultExtraNightPrice = dto.ExtraNight,
			NonReturnDays = dto.NonReturnDays,
			ExpirationPrice = dto.ExpirePrice,
			NonReturn = dto.NonReturnPrice,
			PromoValue = dto.InitialNight,
			Purchase = 0m,
			TitleFamily = titleFamily,
			TitleType = titleType,
			ProductPricingId = dto.ProductPriceId
		};
	}

	public static IPricingRecord ToPricingRecord(this RentalPricingDTO dto, TitleFamily titleFamily, TitleType titleType)
	{
		return new PricingRecord
		{
			InitialDays = dto.InitialDays,
			InitialNight = dto.InitialNight,
			ExtraNight = dto.ExtraNight,
			DefaultInitialNightPrice = dto.DefaultInitialNight,
			DefaultExtraNightPrice = dto.DefaultExtraNight,
			NonReturnDays = dto.NonReturnDays,
			ExpirationPrice = dto.ExpirePrice,
			NonReturn = dto.NonReturnPrice,
			PromoValue = dto.InitialNight,
			Purchase = 0m,
			TitleFamily = titleFamily,
			TitleType = titleType,
			ProductPricingId = dto.ProductPriceId
		};
	}

	public static IPricingRecord ToPricingRecord(this PurchasePricingDTO dto, TitleFamily titleFamily, TitleType titleType)
	{
		return new PricingRecord
		{
			Purchase = dto.PurchasePrice,
			TitleFamily = titleFamily,
			TitleType = titleType,
			ProductPricingId = dto.ProductPriceId
		};
	}
}
