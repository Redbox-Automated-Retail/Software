using System;

namespace KioskClientService.API.Services.Transaction;

public class PromotionInfo
{
	public string PromoCode { get; set; }

	public ReturnPromoType PromoType { get; set; }

	public decimal? Amount { get; set; }

	public DateTime ExpirationDate { get; set; }
}
