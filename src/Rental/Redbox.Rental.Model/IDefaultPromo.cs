namespace Redbox.Rental.Model;

public interface IDefaultPromo
{
	string PromoCode { get; set; }

	string PromotionIntentCode { get; set; }

	double? Amount { get; set; }
}
