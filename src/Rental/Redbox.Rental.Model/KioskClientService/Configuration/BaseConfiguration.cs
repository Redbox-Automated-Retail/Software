using System;
using System.Linq;
using System.Reflection;
using Redbox.Core;

namespace Redbox.Rental.Model.KioskClientService.Configuration;

public class BaseConfiguration
{
	public long ConfigurationVersion { get; set; }

	public void Log(ConfigLogParameters loggingParameters = null)
	{
		LogHelper.Instance.Log($"KioskConfiguration Settings for configuration version: {ConfigurationVersion}");
		PropertyInfo[] properties = GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.PropertyType.IsSubclassOf(typeof(BaseCategorySetting)))
			{
				object categoryClassInstance = GetCategoryClassInstance(propertyInfo);
				LogCategorySettingValues(categoryClassInstance, loggingParameters);
			}
		}
	}

	private void LogCategorySettingValues(object categoryClassInstance, ConfigLogParameters loggingParameters = null)
	{
		if (categoryClassInstance == null)
		{
			return;
		}
		string categoryName = GetCategoryName(categoryClassInstance);
		PropertyInfo[] properties = categoryClassInstance.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite && isSettingIncluded(categoryClassInstance, propertyInfo, loggingParameters))
			{
				string settingName = GetSettingName(propertyInfo);
				object value = propertyInfo.GetValue(categoryClassInstance);
				value = MaskSettingValue(propertyInfo, value);
				LogHelper.Instance.Log($"KioskConfiguration.{categoryName}.{settingName} = {value}");
			}
		}
	}

	private bool isSettingIncluded(object categoryClassInstance, PropertyInfo settingProperty, ConfigLogParameters loggingParameters)
	{
		string categoryName = GetCategoryName(categoryClassInstance);
		string name = categoryClassInstance.GetType().Name;
		string name2 = settingProperty.Name;
		string settingName = GetSettingName(settingProperty);
		bool flag = loggingParameters?.IncludeCategories == null || !loggingParameters.IncludeCategories.Any();
		bool flag2 = loggingParameters?.IncludeCategories != null && loggingParameters.IncludeCategories.Intersect(new string[2] { categoryName, name }).Any();
		bool flag3 = loggingParameters?.ExcludeCategories != null && loggingParameters.ExcludeCategories.Intersect(new string[2] { categoryName, name }).Any();
		bool flag4 = loggingParameters?.IncludeSettings != null && loggingParameters.IncludeSettings.Intersect(new(string, string)[4]
		{
			(categoryName, settingName),
			(categoryName, name2),
			(name, settingName),
			(name, name2)
		}).Any();
		bool flag5 = loggingParameters?.ExcludeSettings != null && loggingParameters.ExcludeSettings.Intersect(new(string, string)[4]
		{
			(categoryName, settingName),
			(categoryName, name2),
			(name, settingName),
			(name, name2)
		}).Any();
		if (!flag4)
		{
			if ((flag && !flag3) | flag2)
			{
				return !flag5;
			}
			return false;
		}
		return true;
	}

	private string GetSettingName(PropertyInfo settingPropertyInfo)
	{
		string name = settingPropertyInfo.Name;
		SettingAttribute settingAttribute = (SettingAttribute)Attribute.GetCustomAttribute(settingPropertyInfo, typeof(SettingAttribute));
		if (settingAttribute != null)
		{
			name = settingAttribute.Name;
		}
		return name;
	}

	private string MaskSettingValue(PropertyInfo settingPropertyInfo, object settingValue)
	{
		string text = $"{settingValue}";
		MaskLogValue maskLogValueAttribute = GetMaskLogValueAttribute(settingPropertyInfo);
		if (maskLogValueAttribute != null && !string.IsNullOrEmpty(text))
		{
			int count = Math.Max(0, text.Length - maskLogValueAttribute.VisibleChars);
			int startIndex = Math.Max(0, text.Length - maskLogValueAttribute.VisibleChars);
			text = new string('X', count) + text.Substring(startIndex);
		}
		return text;
	}

	private MaskLogValue GetMaskLogValueAttribute(PropertyInfo propertyInfo)
	{
		return (MaskLogValue)Attribute.GetCustomAttribute(propertyInfo, typeof(MaskLogValue));
	}

	private string GetCategoryName(object categoryClassInstance)
	{
		string result = categoryClassInstance.GetType().Name;
		CategoryAttribute categoryAttribute = (CategoryAttribute)Attribute.GetCustomAttribute(categoryClassInstance.GetType(), typeof(CategoryAttribute));
		if (!string.IsNullOrEmpty(categoryAttribute?.Name))
		{
			result = categoryAttribute?.Name;
		}
		return result;
	}

	private object GetCategoryClassInstance(PropertyInfo categoryClassPropertyInfo)
	{
		object obj = categoryClassPropertyInfo?.GetValue(this, null);
		if (obj == null)
		{
			try
			{
				obj = Activator.CreateInstance(categoryClassPropertyInfo.PropertyType);
				if (obj != null)
				{
					categoryClassPropertyInfo.SetValue(this, obj);
				}
				else
				{
					LogHelper.Instance.Log("Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name, e);
			}
		}
		return obj;
	}
}
