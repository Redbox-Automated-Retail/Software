using Redbox.Rental.Model.Loyalty;

namespace Redbox.Rental.Services.Loyalty;

public class ProductEstimatePointsAccrued : IProductEstimatedPointsAccrued
{
	public long ProductId { get; set; }

	public ILoyaltyPointsRecord LoyaltyPointsRecord { get; set; }
}
