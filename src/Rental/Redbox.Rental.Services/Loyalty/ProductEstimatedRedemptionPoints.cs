using Redbox.Rental.Model.Loyalty;

namespace Redbox.Rental.Services.Loyalty;

public class ProductEstimatedRedemptionPoints : IProductEstimatedRedemptionPoints
{
	public long ProductId { get; set; }

	public ILoyaltyPointsRecord LoyaltyPointsRecord { get; set; }
}
