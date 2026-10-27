using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Model.Analytics;

public class PricingData : AnalyticsData
{
	public long? ProductId { get; set; }

	public decimal InitialNight { get; set; }

	public decimal ExtraNight { get; set; }

	public long? ProductPricingId { get; set; }

	public int? InitialDays { get; set; }

	public bool IsDeal { get; set; }

	public decimal Purchase { get; set; }

	public decimal? DefaultInitialNightPrice { get; set; }

	public decimal? DefaultExtraNightPrice { get; set; }

	public PricingData()
	{
		base.DataType = "Pricing";
	}

	public static PricingData ToPricingData(long productId, IPricingRecord pricingRecord)
	{
		return new PricingData
		{
			ProductId = productId,
			InitialNight = pricingRecord.InitialNight,
			ExtraNight = pricingRecord.ExtraNight,
			ProductPricingId = pricingRecord.ProductPricingId,
			InitialDays = pricingRecord.InitialDays,
			IsDeal = pricingRecord.IsDeal,
			Purchase = pricingRecord.Purchase,
			DefaultInitialNightPrice = pricingRecord.DefaultInitialNightPrice,
			DefaultExtraNightPrice = pricingRecord.DefaultExtraNightPrice
		};
	}
}
