namespace Redbox.Rental.Model.Analytics;

public class OfferData : AnalyticsData
{
	public string DiscountType { get; set; }

	public string PromotionCode { get; set; }

	public decimal? Amount { get; set; }

	public OfferData(string discountType, string promotionCode, decimal? amount)
	{
		base.DataType = "Offer";
		DiscountType = discountType;
		PromotionCode = promotionCode;
		Amount = amount;
	}
}
