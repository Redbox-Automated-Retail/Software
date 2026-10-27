using System;
using System.Collections.Generic;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public interface IAnalyticsSession
{
	string SessionId { get; set; }

	DateTime StartTime { get; set; }

	DateTime EndTime { get; set; }

	long KioskId { get; set; }

	List<IAnalyticsEvent> Events { get; }

	Dictionary<string, object> Properties { get; }

	List<ABTest> ABTests { get; }

	AnalyticsShoppingCart ShoppingCart { get; set; }

	void End();

	IAnalyticsEvent AddView(string name);

	IAnalyticsEvent AddViewEvent(string action, string name);

	IAnalyticsEvent AddViewEvent(IAnalyticsEvent evt);

	IAnalyticsEvent AddButtonPressEvent(string name, string eventSubType = null);

	IAnalyticsEvent AddCheckBoxEvent(string name, bool isChecked);

	IAnalyticsData AddData(IAnalyticsData analyticsData);

	IAnalyticsSession AddProperty(string name, object value);

	AnalyticsShoppingCart AddShoppingCart(IRentalShoppingCart sc);

	IAnalyticsEvent AddEventProperty(string name, object value);
}
