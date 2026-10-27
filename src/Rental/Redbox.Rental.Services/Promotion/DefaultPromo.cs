using Redbox.Rental.Model;

namespace Redbox.Rental.Services.Promotion;

public class DefaultPromo : IDefaultPromo
{
	public string PromoCode { get; set; }

	public string PromotionIntentCode { get; set; }

	public double? Amount { get; set; }
}
