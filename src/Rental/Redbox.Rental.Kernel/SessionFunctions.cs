using System;
using System.Reflection;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel;

public static class SessionFunctions
{
	private static BaseSettingFunctions _baseSettingFunctions = new BaseSettingFunctions();

	[KernelFunction(Name = "kernel.close_open_session_and_start_new_rental_session")]
	public static void CloseOpenSessionAndStartNewRentalSession(string newSessionContext)
	{
		try
		{
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(newSessionContext);
		}
		catch (Exception ex)
		{
			LogHelper.Instance.LogEntireException("An unhandled exception was raised in SessionFunctions.CloseOpenSessionAndStartNewRentalSession", ex, "CloseOpenSessionAndStartNewRentalSession");
			throw ex;
		}
	}

	[KernelFunction(Name = "kernel.is_rental_session_active")]
	public static bool IsRentalSessionActive()
	{
		return OrchestrationService.IsRentalSessionActive();
	}

	[KernelFunction(Name = "kernel.close_shopping_cart_analytics")]
	public static void CloseShoppingCartAnalytics(string sessionId, string isSessionAbandoned)
	{
		OrchestrationService.CloseShoppingCartAnalytics((sessionId != null) ? new Guid(sessionId) : Guid.NewGuid(), bool.Parse(isSessionAbandoned));
	}

	[KernelFunction(Name = "kernel.get_session_value")]
	public static object GetSessionValue(string valueName)
	{
		object obj = null;
		if (valueName != null)
		{
			return _baseSettingFunctions.GetPropertyValue(Session.Instance, valueName);
		}
		throw new ArgumentNullException("Error in GetSessionValue.  valueName can not be null.");
	}

	[KernelFunction(Name = "kernel.set_session_value")]
	public static void SetPropertyValue(string propertyName, object value)
	{
		_ = Session.Instance;
		object resultObject = null;
		PropertyInfo resultPropertyInfo = null;
		_baseSettingFunctions.GetProperty(Session.Instance, propertyName, out resultObject, out resultPropertyInfo);
		if (resultPropertyInfo != null && resultObject != null)
		{
			object value2 = _baseSettingFunctions.ConvertValueToPropertyType(resultPropertyInfo, value);
			resultPropertyInfo.SetValue(resultObject, value2, null);
			return;
		}
		throw new ArgumentException($"kernel.set_session_value unable to locate property {propertyName}");
	}
}
