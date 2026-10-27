using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Analytics;

public class AnalyticsEvent : IAnalyticsEvent
{
	private List<IAnalyticsEvent> _events;

	private Dictionary<string, object> _properties;

	private List<IAnalyticsData> _data;

	public DateTime Timestamp { get; set; } = DateTime.Now;

	public string EventType { get; set; }

	public string EventSubType { get; set; }

	public List<IAnalyticsEvent> Events => _events;

	public Dictionary<string, object> Properties => _properties;

	public List<IAnalyticsData> Data => _data;

	public IAnalyticsEvent AddEvent(IAnalyticsEvent analyticsEvent)
	{
		if (_events == null)
		{
			_events = new List<IAnalyticsEvent>();
		}
		Events.Add(analyticsEvent);
		return analyticsEvent;
	}

	public IAnalyticsEvent AddProperty(string name, object value)
	{
		if (_properties == null)
		{
			_properties = new Dictionary<string, object>();
		}
		_properties[name] = value;
		return this;
	}

	public IAnalyticsData AddData(IAnalyticsData analyticsData)
	{
		if (analyticsData == null)
		{
			return null;
		}
		if (_data == null)
		{
			_data = new List<IAnalyticsData>();
		}
		Data.Add(analyticsData);
		return analyticsData;
	}
}
