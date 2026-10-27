namespace Redbox.Rental.Model.Loyalty;

public interface ILoyaltyPointsRecord
{
	int? PointsToBeEarned { get; set; }

	int? PointsToRedeem { get; set; }

	bool ToBeRedeemed { get; set; }

	bool CanRedeem { get; }

	void Clone(ILoyaltyPointsRecord source);
}
