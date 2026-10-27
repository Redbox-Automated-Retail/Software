namespace Redbox.Rental.Model.Analytics;

public class DiscountRestrictionEvent : AnalyticsEvent
{
	public string RestrictedPromotionName { get; set; }

	public string OfferedPromotionName { get; set; }

	public DiscountRestrictionEvent(string restrictedPromotionName, string offeredPromotionName)
	{
		base.EventType = "DiscountRestriction";
		RestrictedPromotionName = restrictedPromotionName;
		OfferedPromotionName = offeredPromotionName;
	}
}
