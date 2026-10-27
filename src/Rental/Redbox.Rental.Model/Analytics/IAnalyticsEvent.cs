using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Analytics;

public interface IAnalyticsEvent
{
	DateTime Timestamp { get; set; }

	string EventType { get; }

	List<IAnalyticsEvent> Events { get; }

	List<IAnalyticsData> Data { get; }

	IAnalyticsEvent AddEvent(IAnalyticsEvent analyticsEvent);

	IAnalyticsEvent AddProperty(string name, object value);

	IAnalyticsData AddData(IAnalyticsData analyticsData);
}
