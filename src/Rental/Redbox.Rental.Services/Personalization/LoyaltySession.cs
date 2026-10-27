using System;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class LoyaltySession : ILoyaltySession
{
	public int PointsBalance { get; set; }

	public LoyaltyTiers LoyaltyTier { get; set; }

	public bool SignInError { get; set; }

	public string FirstName { get; set; }

	public string LoginEmail { get; set; }

	public string PhoneNumber { get; set; }

	public string PostalCode { get; set; }

	public int? BirthDayOfMonth { get; set; }

	public int? BirthMonth { get; set; }

	public string LoyaltyTierName { get; set; }

	public int LoyaltyTierCounter { get; set; }

	public int PointsExpiring { get; set; }

	public DateTime? PointsExpirationDate { get; set; }

	public string CustomerProfileNumber { get; set; }
}
