using System.Collections.Generic;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class PersonalizationSession : IPersonalizationSession, ILoginEmailProvider
{
	public string CustomerProfileNumber { get; set; }

	public string LoginEmailAddress { get; set; }

	public string FirstName { get; set; }

	public bool IsLoggedIn { get; set; }

	public bool PromptForPerks { get; set; }

	public bool PromptForEarlyId { get; set; }

	public bool TextClubEnabled { get; set; }

	public string MobilePhoneNumber { get; set; }

	public bool IsCPNewInThisSession { get; set; }

	public ILoyaltySession LoyaltySession { get; set; }

	public IList<IRecommendedTitles> RecommendedTitles { get; set; }

	public StoredPromoCodeCollection StoredPromoCodes { get; set; }

	public IList<IAcceptedOffer> AcceptedOffers { get; set; }

	public string SelectedPromo { get; set; }

	public IRedboxPlusSession RedboxPlusSession { get; set; }

	public ISubscriptionData SubscriptionData { get; set; }

	public void LogOut()
	{
		IsLoggedIn = false;
		LogOutLoyaltySession();
		LogOutRedboxPlusSession();
	}

	public void LogOutLoyaltySession()
	{
		LoyaltySession = null;
	}

	public void LogOutRedboxPlusSession()
	{
		RedboxPlusSession = null;
	}
}
