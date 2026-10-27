using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;

namespace Redbox.Rental.Kernel;

public static class AnalyticsServiceFunctions
{
	[KernelFunction(Name = "kernel.analyticsserviceaddauthtransactionInfo")]
	internal static void AnalyticsAddAuthTransactionInfo(string customerProfileNumber, string accountNumber, string transactionId, string sessionId)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddAuthTransactionInfo(customerProfileNumber, accountNumber, transactionId, sessionId);
	}

	[KernelFunction(Name = "kernel.analyticsserviceaddropflowfalse")]
	internal static void AnalyticsAddROPFlowFalse()
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddROPFlowFalse();
	}

	[KernelFunction(Name = "kernel.analyticsserviceaddeventviewshown")]
	internal static void AddEventViewShown(string sessionId)
	{
		IAnalyticsService service = ServiceLocator.Instance.GetService<IAnalyticsService>();
		IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
		service?.AddView(service2?.GetCurrentViewAnalyticsName());
	}

	[KernelFunction(Name = "kernel.analyticsserviceaddbuttonpressevent")]
	internal static void AddButtonPressEvent(string name, string eventSubType = null)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>().AddButtonPressEvent(name, eventSubType);
	}

	[KernelFunction(Name = "kernel.analyticsserviceaddeventauthorization")]
	internal static void AddEventAuthorization(string sessionId, string type, bool online, bool? approved, bool? pickupValid)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddData(new AuthorizationData
		{
			AuthorizationType = type,
			Online = online,
			Approved = approved,
			PickupValid = pickupValid
		});
	}

	[KernelFunction(Name = "kernel.analyticsservicesessionexists")]
	internal static bool SessionExists(string sessionId)
	{
		return ServiceLocator.Instance.GetService<IAnalyticsService>()?.SessionExists(sessionId) ?? false;
	}
}
