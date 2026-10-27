namespace Redbox.Rental.Model.Analytics;

public class ViewEventData : AnalyticsEvent
{
	public string Action { get; set; }

	public string Name { get; set; }

	public ViewEventData(string action, string name)
	{
		base.EventType = "ViewEvent";
		Action = action;
		Name = name;
	}
}
