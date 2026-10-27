namespace KioskClientService.API.Services.Configuration;

public class KioskClientSubscription : BaseCategorySetting
{
	public bool SubscriptionPlansActive { get; set; }

	public string BrowseStarzSubscriptionPlan { get; set; }

	public string MyBagStarzSubscriptionPlan { get; set; }
}
