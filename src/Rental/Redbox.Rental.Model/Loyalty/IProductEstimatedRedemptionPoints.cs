namespace Redbox.Rental.Model.Loyalty;

public interface IProductEstimatedRedemptionPoints
{
	long ProductId { get; set; }

	ILoyaltyPointsRecord LoyaltyPointsRecord { get; set; }
}
