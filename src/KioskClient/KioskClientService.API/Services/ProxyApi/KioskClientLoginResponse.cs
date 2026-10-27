using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskClientLoginResponse
{
	public string CustomerProfileNumber { get; set; }

	public string FirstName { get; set; }

	public string LoginEmail { get; set; }

	public string PhoneNumber { get; set; }

	public string PostalCode { get; set; }

	public int? BirthMonth { get; set; }

	public int? BirthDayOfMonth { get; set; }

	public int LoyaltyPointBalance { get; set; }

	public string LoyaltyTier { get; set; }

	public int LoyaltyTierCounter { get; set; }

	public int PointsExpiring { get; set; }

	public DateTime? PointsExpirationDate { get; set; }

	public bool IsVerified { get; set; }

	public string MobilePhoneNumber { get; set; }

	public bool PromptForPerks { get; set; }

	public bool PromptForEarlyId { get; set; }

	public bool TextClubEnabled { get; set; }

	public bool? IsEmailVerified { get; set; }

	public bool IsLoyaltyOnline { get; set; }

	public KioskClientRecommendedTitles RecommendedTitles { get; set; }

	public List<KioskClientStoredPromoCode> StoredPromoCodes { get; set; }

	public List<KioskClientAcceptedOffer> AcceptedOffers { get; set; }

	public bool Success { get; set; }

	public int StatusCode { get; set; }
}
