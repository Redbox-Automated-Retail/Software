namespace Redbox.Rental.Model.Analytics;

public class TimeoutEvent : AnalyticsEvent
{
	public string ViewName { get; set; }

	public int Timeout { get; set; }

	public bool TTSEnabled { get; set; }

	public TimeoutEvent(string viewName, int timeout, bool ttsEnabled = false)
	{
		base.EventType = "Timeout";
		ViewName = viewName;
		Timeout = timeout;
		TTSEnabled = ttsEnabled;
	}
}
