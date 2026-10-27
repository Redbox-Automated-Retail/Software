namespace Redbox.Rental.Model.KioskClientService.Promotion;

public class DefaultPromo : IDefaultPromo
{
	public string PromoCode { get; set; }

	public string PromotionIntentCode { get; set; }

	public double? Amount { get; set; }
}
