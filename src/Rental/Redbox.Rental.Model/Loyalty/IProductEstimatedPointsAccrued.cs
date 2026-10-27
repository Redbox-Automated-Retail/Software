namespace Redbox.Rental.Model.Loyalty;

public interface IProductEstimatedPointsAccrued
{
	long ProductId { get; set; }

	ILoyaltyPointsRecord LoyaltyPointsRecord { get; set; }
}
