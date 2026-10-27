namespace Redbox.Rental.Model.Analytics;

public class NameValueData : AnalyticsData
{
	public string Name { get; set; }

	public string Value { get; set; }

	public NameValueData(string name, string value)
	{
		Name = name;
		Value = value;
		base.DataType = "NameValue";
	}
}
