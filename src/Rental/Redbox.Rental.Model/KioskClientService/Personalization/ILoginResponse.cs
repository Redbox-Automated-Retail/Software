using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Personalization;

public interface ILoginResponse : IBaseResponse
{
	List<AcceptedOffer> AcceptedOffers { get; set; }

	int? BirthDayOfMonth { get; set; }

	int? BirthMonth { get; set; }

	string CustomerProfileNumber { get; set; }

	string FirstName { get; set; }

	bool? IsEmailVerified { get; set; }

	bool PromptForPerks { get; set; }

	bool PromptForEarlyId { get; set; }

	bool TextClubEnabled { get; set; }

	bool IsVerified { get; set; }

	string LoginEmail { get; set; }

	int LoyaltyPointBalance { get; set; }

	string LoyaltyTier { get; set; }

	int LoyaltyTierCounter { get; set; }

	string MobilePhoneNumber { get; set; }

	string PhoneNumber { get; set; }

	DateTime? PointsExpirationDate { get; set; }

	int PointsExpiring { get; set; }

	string PostalCode { get; set; }

	bool IsLoyaltyOnline { get; set; }

	List<StoredPromoCode> StoredPromoCodes { get; set; }

	RecommendedTitles RecommendedTitles { get; set; }
}
