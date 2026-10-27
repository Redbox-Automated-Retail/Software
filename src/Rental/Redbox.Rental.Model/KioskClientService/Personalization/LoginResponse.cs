using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Personalization;

public class LoginResponse : BaseResponse, ILoginResponse, IBaseResponse
{
	private readonly Dictionary<string, object> _response;

	public string CustomerProfileNumber { get; set; }

	public string FirstName { get; set; }

	public string LoginEmail { get; set; }

	public string PhoneNumber { get; set; }

	public string PostalCode { get; set; }

	public int? BirthDayOfMonth { get; set; }

	public int? BirthMonth { get; set; }

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

	public RecommendedTitles RecommendedTitles { get; set; }

	public List<StoredPromoCode> StoredPromoCodes { get; set; }

	public List<AcceptedOffer> AcceptedOffers { get; set; }

	public LoginResponse(Dictionary<string, object> response)
	{
		_response = response;
	}
}
