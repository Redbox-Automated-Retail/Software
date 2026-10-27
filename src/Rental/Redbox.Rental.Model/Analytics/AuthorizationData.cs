namespace Redbox.Rental.Model.Analytics;

public class AuthorizationData : AnalyticsData
{
	public string AuthorizationType { get; set; }

	public bool Online { get; set; }

	public bool? Approved { get; set; }

	public bool? PickupValid { get; set; }

	public AuthorizationData()
	{
		base.DataType = "Authorization";
	}
}
