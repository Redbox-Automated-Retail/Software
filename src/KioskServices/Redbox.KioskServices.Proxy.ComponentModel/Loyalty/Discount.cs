using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel.Loyalty;

public class Discount
{
	public long? ProductId { get; set; }

	public DiscountType DiscountType { get; set; }

	public string PromotionCode { get; set; }

	public decimal? PromotionCodeValue { get; set; }

	public PromotionIntentCode? PromotionIntentCode { get; set; }

	public int? RedemptionPoints { get; set; }

	public decimal Amount { get; set; }

	public bool? ApplyOnlyToProduct { get; set; }
}
