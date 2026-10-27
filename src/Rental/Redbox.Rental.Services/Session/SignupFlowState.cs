using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class SignupFlowState : ISignupFlowState
{
	public bool MarketingOptInShown { get; set; }

	public bool EmailOptInCampaignShown { get; set; }

	public bool WasPerksSignupOfferPresented { get; set; }

	public bool WasEarlyIdSignupPresented { get; set; }
}
