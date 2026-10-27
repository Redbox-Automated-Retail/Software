using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Model.Analytics;

public class PurchasePricingData : AnalyticsData
{
	public long? ProductId { get; set; }

	public long? ProductPricingId { get; set; }

	public decimal Purchase { get; set; }

	public PurchasePricingData()
	{
		base.DataType = "PurchasePricing";
	}

	public static PricingData ToPricingData(long productId, IPricingRecord pricingRecord)
	{
		return new PricingData
		{
			ProductId = productId,
			ProductPricingId = pricingRecord.ProductPricingId,
			Purchase = pricingRecord.Purchase
		};
	}
}
