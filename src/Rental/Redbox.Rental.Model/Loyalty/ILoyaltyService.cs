using System;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Loyalty;

public interface ILoyaltyService
{
	void EstimatePointsAccrued(IRentalShoppingCart rentalShoppingCart, Action<IEstimatePointsAccruedResult> estimatPointsAccuedResultCallback);

	void EstimateRedemptionPoints(IRentalShoppingCart rentalShoppingCart, Action<IEstimateRedemptionPointsResult> estimatRedemptionPointsResultCallback);
}
