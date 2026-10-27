using System.Collections.Generic;

namespace Redbox.Rental.Model.Loyalty;

public interface IEstimateRedemptionPointsResult
{
	bool Success { get; set; }

	List<IProductEstimatedRedemptionPoints> ProductEstimatedRedemptionPointsList { get; set; }

	string ErrorMessage { get; set; }
}
