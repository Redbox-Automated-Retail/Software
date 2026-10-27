namespace Redbox.Rental.Model.KioskClientService.Subscriptions;

public interface IRedboxPlusPromoCampaign
{
	int PromoCampaignId { get; set; }

	RedboxPlusSubscriptionTier Tier { get; set; }
}
