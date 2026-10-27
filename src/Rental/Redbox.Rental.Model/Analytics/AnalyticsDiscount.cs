using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Analytics;

public class AnalyticsDiscount
{
	public long? ProductId { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public DiscountType DiscountType { get; set; }

	public string PromotionCode { get; set; }

	public decimal? PromotionCodeValue { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public PromotionIntentCode? PromotionIntentCode { get; set; }

	public int? RedemptionPoints { get; set; }

	public decimal Amount { get; set; }

	public bool? ApplyOnlyToProduct { get; set; }
}
