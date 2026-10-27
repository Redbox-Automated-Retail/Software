using System;

namespace KioskClientService.API.Services.ProxyApi;

public class StoredPromoCode
{
	public string Code { get; set; }

	public string CampaignName { get; set; }

	public string PromotionUsage { get; set; }

	public string RedemptionType { get; set; }

	public DateTime ExpirationDate { get; set; }

	public string ExpirationText { get; set; }

	public int? DaysUntilExpiration { get; set; }
}
