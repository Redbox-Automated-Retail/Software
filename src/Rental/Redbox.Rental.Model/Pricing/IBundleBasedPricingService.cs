using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Pricing;

public interface IBundleBasedPricingService
{
	IPricingSet CurrentPriceSet { get; }

	bool Reload();

	IPricingRecord GetProductPricingRecord(long productId);

	IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool useAdjustedPrices);
}
