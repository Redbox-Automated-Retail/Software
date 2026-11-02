using System;
using System.Reflection;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public class ApplicationStateFunctions
{
	private static BaseSettingFunctions _baseSettingFunctions = new BaseSettingFunctions();

	[KernelFunction(Name = "kernel.get_application_state_value")]
	public static object? GetSessionValue(string valueName)
	{
		if (valueName != null)
		{
			return _baseSettingFunctions.GetPropertyValue(ApplicationState.Instance, valueName);
		}
		throw new ArgumentNullException("Error in GetSessionValue.  valueName can not be null.");
	}

	[KernelFunction(Name = "kernel.set_application_state_value")]
	public static void SetPropertyValue(string propertyName, object value)
	{
		_ = ApplicationState.Instance;
		object? resultObject = null;
		PropertyInfo? resultPropertyInfo = null;
		_baseSettingFunctions.GetProperty(ApplicationState.Instance, propertyName, out resultObject, out resultPropertyInfo);
		if (resultPropertyInfo != null && resultObject != null)
		{
			object? convertedValue = _baseSettingFunctions.ConvertValueToPropertyType(resultPropertyInfo, value);
			resultPropertyInfo.SetValue(resultObject, convertedValue, null);
			return;
		}
		throw new ArgumentException($"kernel.set_application_state_value unable to locate property {propertyName}");
	}
}
