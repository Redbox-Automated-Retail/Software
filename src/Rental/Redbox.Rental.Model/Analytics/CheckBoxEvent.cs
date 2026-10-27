namespace Redbox.Rental.Model.Analytics;

public class CheckBoxEvent : AnalyticsEvent
{
	public string Name { get; set; }

	public bool IsChecked { get; set; }

	public CheckBoxEvent(string name, bool isChecked)
		: this(null, name)
	{
		IsChecked = isChecked;
	}

	public CheckBoxEvent(string eventSubType, string name)
	{
		base.EventType = "CheckBoxPressed";
		Name = name;
		if (!string.IsNullOrWhiteSpace(eventSubType))
		{
			base.EventSubType = eventSubType;
		}
	}
}
