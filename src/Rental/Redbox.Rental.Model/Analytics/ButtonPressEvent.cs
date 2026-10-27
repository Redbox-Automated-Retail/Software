namespace Redbox.Rental.Model.Analytics;

public class ButtonPressEvent : AnalyticsEvent
{
	public string Name { get; set; }

	public ButtonPressEvent(string name)
		: this(name, null)
	{
	}

	public ButtonPressEvent(string name, string eventSubType)
	{
		base.EventType = "ButtonPress";
		Name = name;
		if (!string.IsNullOrWhiteSpace(eventSubType))
		{
			base.EventSubType = eventSubType;
		}
	}
}
