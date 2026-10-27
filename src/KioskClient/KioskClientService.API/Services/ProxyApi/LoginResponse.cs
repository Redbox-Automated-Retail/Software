using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class LoginResponse : PersonalizationBaseResponse
{
	public string CustomerProfileNumber { get; set; }

	public string FirstName { get; set; }

	public string LoginEmail { get; set; }

	public string PhoneNumber { get; set; }

	public string PostalCode { get; set; }

	public DateTime? BirthDate { get; set; }

	public int LoyaltyPointBalance { get; set; }

	public string LoyaltyTier { get; set; }

	public int LoyaltyTierCounter { get; set; }

	public int PointsExpiring { get; set; }

	public DateTime? PointsExpirationDate { get; set; }

	public Dictionary<string, string> Attributes { get; set; }

	public bool IsVerified { get; set; }

	public string MobilePhoneNumber { get; set; }

	public string EarlyIdOptInStatus { get; set; }

	public string TextClubOptInStatus { get; set; }

	public bool IsEnrolledInLoyalty { get; set; }

	public bool? IsEmailVerified { get; set; }

	public RecommendedTitles RecommendedTitles { get; set; }

	public List<StoredPromoCode> PromoCodes { get; set; }

	public List<AcceptedOffer> AcceptedOffers { get; set; }
}
