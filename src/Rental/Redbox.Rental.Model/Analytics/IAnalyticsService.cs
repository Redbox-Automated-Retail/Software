using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public interface IAnalyticsService
{
	IAnalyticsSession CurrentSession { get; set; }

	IAnalyticsSession Start(string sessionId, AnalyticsConstants.SessionType sessionType);

	void End(string sessionId, bool isAbandoned);

	void Remove(string sessionId);

	IAnalyticsEvent AddView(string name);

	void AddQueuedViewEvents();

	IAnalyticsEvent AddViewEvent(string action, string name);

	IAnalyticsData AddData(IAnalyticsData analyticsData);

	IAnalyticsSession AddSessionProperty(string name, object value);

	IAnalyticsSession AddSessionProperty(string name, object value, string sessionId);

	IAnalyticsEvent AddEventProperty(string name, object value);

	IAnalyticsSession AddAuthTransactionInfo(string customerProfileNumber, string accountNumber, string transactionId, string sessionId);

	IAnalyticsSession AddAuthTransactionInfo(IAnalyticsSession session, string customerProfileNumber, string accountNumber, string transactionId);

	IAnalyticsSession AddROPFlowFalse();

	AnalyticsShoppingCart AddShoppingCart(IRentalShoppingCart sc);

	IAnalyticsEvent AddViewEvent(IAnalyticsEvent evt);

	IAnalyticsEvent AddOrQueueViewEvent(IAnalyticsEvent evt);

	IAnalyticsEvent AddButtonPressEvent(string name, string eventSubType = null);

	IAnalyticsEvent AddCheckBoxEvent(string name, bool isChecked);

	IAnalyticsEvent AddTimeoutEvent(int timeOut);

	IAnalyticsEvent AddTimeoutEvent(string viewName, int timeOut);

	IAnalyticsSession AddCurrentCulture(string culture);

	void WriteToFile(string sessionId, string contents, string extension);

	void SendToKioskDataService(string sessionId);

	bool SessionExists(string sessionId);
}
