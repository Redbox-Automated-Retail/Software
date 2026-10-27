namespace Redbox.Rental.Model.Analytics;

public class ViewShownEvent : AnalyticsEvent
{
	public string ViewName { get; set; }

	public ViewShownEvent(string name)
	{
		base.EventType = "View";
		ViewName = name;
	}
}
