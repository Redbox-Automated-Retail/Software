using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class SessionAnalyticsFunctions
{
	[KernelFunction(Name = "kernel.analyticssessionexists")]
	internal static bool AnalyticsSessionExists(string sessionId)
	{
		return false;
	}

	[KernelFunction(Name = "kernel.analyticsaddview")]
	internal static void AnalyticsAddView(string sessionId, string viewName)
	{
	}

	[KernelFunction(Name = "kernel.analyticsclose")]
	internal static void AnalyticsClose(string sessionId, string isSessionAbandoned)
	{
	}

	[KernelFunction(Name = "kernel.analyticsdisposesession")]
	internal static void AnalyticsDisposeSession(string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddnode")]
	internal static void AnalyticsAddNode(string parentNodeName, string nodeName, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddnodewithattributes")]
	internal static void AnalyticsAddNode(string parentNodeName, string nodeName, LuaTable table, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddelement")]
	internal static void AnalyticsAddElement(string parentNodeName, string elementName, string elementValue, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddelements")]
	internal static void AnalyticsAddElements(string parentNodeName, LuaTable table, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddorupdateelement")]
	internal static void AnalyticsAddOrUpdateElement(string parentNodeName, string elementName, string elementValue, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddauthtransactionInfo")]
	internal static void AnalyticsAddAuthTransactionInfo(string customerProfileNumber, string accountNumber, string transactionId, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsremoveelement")]
	internal static void AnalyticsRemoveElement(string parentNodeName, string elementName, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddattribute")]
	internal static void AnalyticsAddAttribute(string parentNodeName, string attributeName, string attributeValue, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddattributes")]
	internal static void AnalyticsAddAttributes(string parentNodeName, LuaTable table, string sessionId)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddviewevent")]
	internal static void AnalyticsAddViewEvent(string subItemType, string subItemValue)
	{
	}

	[KernelFunction(Name = "kernel.analyticsaddviewevent2")]
	internal static void AnalyticsAddViewEvent2(string subItemType, string subItemValue, string sessionId)
	{
	}
}
