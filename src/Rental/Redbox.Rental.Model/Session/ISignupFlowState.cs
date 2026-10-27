namespace Redbox.Rental.Model.Session;

public interface ISignupFlowState
{
	bool MarketingOptInShown { get; set; }

	bool WasPerksSignupOfferPresented { get; set; }

	bool WasEarlyIdSignupPresented { get; set; }
}
