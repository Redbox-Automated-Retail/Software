using System;

namespace Redbox.Rental.Model.KioskClientService.Personalization;

public class StoredPromoCode
{
	public string PromoCode { get; set; }

	public string CampaignName { get; set; }

	public string UsageDescription { get; set; }

	public string RedemptionTypeDescription { get; set; }

	public DateTime ExpirationDate { get; set; }

	public string ExpirationText { get; set; }

	public int? DaysUntilExpiration { get; set; }
}
