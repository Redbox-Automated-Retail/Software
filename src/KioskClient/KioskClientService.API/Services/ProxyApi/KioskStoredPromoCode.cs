using System;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskStoredPromoCode
{
	public string Code { get; set; }

	public int CampaignId { get; set; }

	public string CampaignName { get; set; }

	public string PromotionUsage { get; set; }

	public string RedemptionType { get; set; }

	public DateTime ExpirationDate { get; set; }
}
