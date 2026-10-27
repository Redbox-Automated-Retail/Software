using System;

namespace Redbox.Rental.Model.Personalization;

public interface IStoredPromoCode
{
	string PromoCode { get; set; }

	int CampaignId { get; set; }

	string CampaignName { get; set; }

	string UsageDescription { get; set; }

	string RedemptionTypeDescription { get; set; }

	DateTime ExpirationDate { get; set; }

	string ExpirationText { get; set; }

	int? DaysUntilExpiration { get; set; }
}
