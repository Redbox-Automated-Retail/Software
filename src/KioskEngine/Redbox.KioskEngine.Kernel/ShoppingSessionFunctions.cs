using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class ShoppingSessionFunctions
{
	internal const string LegacyActiveSessionId = "__active__";

	[KernelFunction(Name = "kernel.sessionstartnew")]
	internal static void StartNewSession(string storeNumber)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.StartNewSession(storeNumber, "__active__");
	}

	[KernelFunction(Name = "kernel.sessionaddevent")]
	internal static void AddNewEvent(string type, string description)
	{
		AddNewEvent("__active__", type, description);
	}

	[KernelFunction(Name = "kernel.sessionsetpostalcode")]
	internal static void SetPostalCode(string postalCode)
	{
		SetPostalCode("__active__", postalCode);
	}

	[KernelFunction(Name = "kernel.sessionsetemail")]
	internal static void SetEmail(string email)
	{
		SetEmail("__active__", email);
	}

	[KernelFunction(Name = "kernel.sessionend")]
	internal static void SessionEnd()
	{
		SessionEnd("__active__");
	}

	[KernelFunction(Name = "kernel.sessionsetcarttype")]
	internal static void SessionSetCartType(string type)
	{
		SessionSetCartType("__active__", type);
	}

	[KernelFunction(Name = "kernel.sessionstartnew2")]
	internal static string StartNewSession2(string storeNumber)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		string result = null;
		if (service != null)
		{
			result = service.StartNewSession(storeNumber, null);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.sessionaddevent2")]
	internal static void AddNewEvent(string sessionId, string type, string description)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.AddEvent(Enum<ShoppingSessionEventType>.ParseIgnoringCase(type, ShoppingSessionEventType.Commit), description);
	}

	[KernelFunction(Name = "kernel.sessionsetpostalcode2")]
	internal static void SetPostalCode(string sessionId, string postalCode)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetPostalCode(postalCode);
	}

	[KernelFunction(Name = "kernel.sessionsetemail2")]
	internal static void SetEmail(string sessionId, string email)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetEmail(email);
	}

	[KernelFunction(Name = "kernel.sessionend2")]
	internal static void SessionEnd(string sessionId)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.End();
	}

	[KernelFunction(Name = "kernel.sessionsetcarttype2")]
	internal static void SessionSetCartType(string sessionId, string type)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId)?.ShoppingCart.SetType(Enum<ShoppingCartType>.ParseIgnoringCase(type, ShoppingCartType.Online));
	}

	[KernelFunction(Name = "kernel.sessionsetcurrent")]
	internal static void SetCurrentSessionId(string sessionId)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>()?.SetCurrentSession(sessionId);
	}
}
