using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Pricing;

public interface IKioskPricingService
{
	bool IsPricingEngineDataAvailable { get; }

	void UpdatePricingInstance(bool force = false);

	IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType);

	IPricingRecord GetActiveProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType);

	IPricingRecord GetOneDayProductPricingRecord(long productId);

	List<IPricingRecord> GetProductPricingRecords(long productId);

	IPricingRecord GetPurchasePricing(ITitleProduct titleProduct);
}
