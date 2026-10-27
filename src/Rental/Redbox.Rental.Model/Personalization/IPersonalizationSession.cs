using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization;

public interface IPersonalizationSession : ILoginEmailProvider
{
	string CustomerProfileNumber { get; set; }

	new string LoginEmailAddress { get; set; }

	string FirstName { get; set; }

	bool IsLoggedIn { get; set; }

	bool PromptForPerks { get; set; }

	bool PromptForEarlyId { get; set; }

	bool TextClubEnabled { get; set; }

	string MobilePhoneNumber { get; set; }

	bool IsCPNewInThisSession { get; set; }

	ILoyaltySession LoyaltySession { get; set; }

	IList<IRecommendedTitles> RecommendedTitles { get; set; }

	StoredPromoCodeCollection StoredPromoCodes { get; set; }

	IList<IAcceptedOffer> AcceptedOffers { get; set; }

	string SelectedPromo { get; set; }

	IRedboxPlusSession RedboxPlusSession { get; set; }

	ISubscriptionData SubscriptionData { get; set; }

	void LogOut();

	void LogOutLoyaltySession();

	void LogOutRedboxPlusSession();
}
