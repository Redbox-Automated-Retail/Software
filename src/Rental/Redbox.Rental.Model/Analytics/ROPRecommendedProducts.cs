using System.Collections.Generic;

namespace Redbox.Rental.Model.Analytics;

public class ROPRecommendedProducts : AnalyticsData
{
	public bool RecommendationsFlow { get; set; }

	public List<ProductData> RecommendedProducts { get; set; }

	public ROPRecommendedProducts()
	{
		base.DataType = "ROPRecommendedProducts";
	}
}
