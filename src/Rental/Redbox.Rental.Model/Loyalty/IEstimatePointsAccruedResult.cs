using System.Collections.Generic;

namespace Redbox.Rental.Model.Loyalty;

public interface IEstimatePointsAccruedResult
{
	bool Success { get; set; }

	List<IProductEstimatedPointsAccrued> ProductEstimatedPointsAccruedList { get; set; }

	string ErrorMessage { get; set; }
}
