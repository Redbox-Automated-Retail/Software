using System.Text;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class Discount
{
	public long ProductId { get; set; }

	public DiscountType DiscountType { get; set; }

	public string PromotionCode { get; set; }

	public decimal? PromotionCodeValue { get; set; }

	public PromotionIntentCode? PromotionIntentCode { get; set; }

	public int? RedemptionPoints { get; set; }

	public decimal Amount { get; set; }

	public short Status { get; set; }

	public bool? ApplyOnlyToProduct { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"ProductId: {ProductId}, DiscountType: {DiscountType}, ");
		if (DiscountType == DiscountType.PromotionCode)
		{
			stringBuilder.Append($"PromotionCode: {PromotionCode}, PromotionIntentCode: {PromotionIntentCode}, PromotionCodeValue: {PromotionCodeValue:C2}, ");
		}
		else if (DiscountType == DiscountType.Loyalty)
		{
			stringBuilder.Append($"RedemptionPoints: {RedemptionPoints}, ");
		}
		stringBuilder.Append($"Amount: {Amount:C2}, Status:{Status}");
		return stringBuilder.ToString();
	}
}
