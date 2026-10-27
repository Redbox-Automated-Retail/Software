using System.Collections.Generic;
using Redbox.Rental.Model.Loyalty;

namespace Redbox.Rental.Services.Loyalty;

public class EstimatedPointsAccruedResult : IEstimatePointsAccruedResult
{
	public bool Success { get; set; }

	public List<IProductEstimatedPointsAccrued> ProductEstimatedPointsAccruedList { get; set; }

	public string ErrorMessage { get; set; }
}
