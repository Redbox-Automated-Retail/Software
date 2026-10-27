using Redbox.Rental.Model.KioskClientService.Subscriptions;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class RedboxPlusPromoCampaign : IRedboxPlusPromoCampaign
{
	public int PromoCampaignId { get; set; }

	public RedboxPlusSubscriptionTier Tier { get; set; }
}
