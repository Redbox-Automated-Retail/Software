using System.Collections.Generic;
using Redbox.Rental.Model.Loyalty;

namespace Redbox.Rental.Services.Loyalty;

public class EstimateRedemptionPointsResult : IEstimateRedemptionPointsResult
{
	public bool Success { get; set; }

	public List<IProductEstimatedRedemptionPoints> ProductEstimatedRedemptionPointsList { get; set; }

	public string ErrorMessage { get; set; }
}
