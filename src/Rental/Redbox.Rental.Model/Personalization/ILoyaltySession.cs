using System;

namespace Redbox.Rental.Model.Personalization;

public interface ILoyaltySession
{
	bool SignInError { get; set; }

	LoyaltyTiers LoyaltyTier { get; set; }

	int PointsBalance { get; set; }

	string FirstName { get; set; }

	string LoginEmail { get; set; }

	string LoyaltyTierName { get; set; }

	int LoyaltyTierCounter { get; set; }

	int PointsExpiring { get; set; }

	DateTime? PointsExpirationDate { get; set; }

	string CustomerProfileNumber { get; set; }
}
