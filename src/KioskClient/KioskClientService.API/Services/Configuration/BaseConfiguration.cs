using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KioskClientService.API.Services.Encryption;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.Configuration;

public abstract class BaseConfiguration
{
	protected readonly ILogger<BaseConfiguration> _logger;

	protected readonly IEncryptionService _encryptionService;

	protected KioskConfigurationSettings _kioskConfigurationSettings;

	private Dictionary<(string categoryName, string settingName), object> _defaultValues;

	protected IOptionsMonitor<KioskConfigurationSettings> _optionsMonitorKioskConfigurationSettings;

	public long ConfigurationVersion => _kioskConfigurationSettings?.ConfigurationVersion ?? 0;

	public BaseConfiguration(ILogger<BaseConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings, IEncryptionService encryptionService)
	{
		_logger = logger;
		_encryptionService = encryptionService;
		_kioskConfigurationSettings = optionsKioskConfigurationSettings.Value;
		PopulateConfigurationSettingValues();
	}

	public BaseConfiguration(ILogger<BaseConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings, IEncryptionService encryptionService)
	{
		_logger = logger;
		_encryptionService = encryptionService;
		_optionsMonitorKioskConfigurationSettings = optionsMonitorKioskConfigurationSettings;
		_optionsMonitorKioskConfigurationSettings.OnChange(OnChangeKioskConfigurationSettings);
		_kioskConfigurationSettings = optionsMonitorKioskConfigurationSettings.CurrentValue;
		PopulateConfigurationSettingValues();
	}

	private void OnChangeKioskConfigurationSettings(KioskConfigurationSettings kioskConfigurationSettings, string data)
	{
		_kioskConfigurationSettings = kioskConfigurationSettings;
		PopulateConfigurationSettingValues();
	}

	public void Log(ConfigLogParameters loggingParameters = null)
	{
		_logger.LogInformation($"KioskConfiguration Settings for configuration version: {_kioskConfigurationSettings.ConfigurationVersion}");
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
			if (!propertyInfo.CanWrite || !(propertyInfo.Name != "ParentKioskConfiguration") || !isSettingIncluded(categoryClassInstance, propertyInfo, loggingParameters))
			{
				continue;
			}
			string settingName = GetSettingName(propertyInfo);
			object settingValue = null;
			bool flag = false;
			KioskSettingValue kioskSettingValue = _kioskConfigurationSettings.GetKioskSettingValue(categoryName, settingName);
			if (kioskSettingValue != null)
			{
				try
				{
					string text = kioskSettingValue.Value;
					bool flag2 = true;
					if (kioskSettingValue.EncryptionType.HasValue)
					{
						if (GetMaskLogValueAttribute(propertyInfo) != null)
						{
							text = _encryptionService.Decrypt(kioskSettingValue.EncryptionType.Value, text).Result;
						}
						else
						{
							flag2 = false;
							text = "[Encrypted]";
						}
					}
					settingValue = (flag2 ? Convert.ChangeType(text, propertyInfo.PropertyType) : text);
				}
				catch (Exception ex)
				{
					flag = true;
					_logger.LogErrorWithSource(ex, "Unable to get " + categoryName + " property " + settingName, "LogCategorySettingValues");
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				try
				{
					if (_defaultValues.TryGetValue((categoryName, settingName), out var value))
					{
						settingValue = value;
					}
				}
				catch (Exception ex2)
				{
					_logger.LogErrorWithSource(ex2, "Unable to get " + categoryName + " property " + settingName + " with default value.", "LogCategorySettingValues");
				}
			}
			settingValue = MaskSettingValue(propertyInfo, settingValue);
			string text2 = (flag ? " [Default value]" : null);
			_logger.LogInformation($"KioskConfiguration.{categoryName}.{settingName} = {settingValue}{text2}");
		}
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

	private void PopulateConfigurationSettingValues()
	{
		try
		{
			if (_optionsMonitorKioskConfigurationSettings != null)
			{
				_kioskConfigurationSettings = _optionsMonitorKioskConfigurationSettings.CurrentValue;
			}
			bool flag = _defaultValues == null;
			if (flag)
			{
				_defaultValues = new Dictionary<(string, string), object>();
			}
			PropertyInfo[] properties = GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.PropertyType.IsSubclassOf(typeof(BaseCategorySetting)))
				{
					object categoryClassInstance = GetCategoryClassInstance(propertyInfo);
					PopulateCategorySettingValues(categoryClassInstance, flag);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while populating configuration setting values.", "PopulateConfigurationSettingValues");
		}
	}

	private void PopulateCategorySettingValues(object categoryClassInstance, bool populateDefaultValues)
	{
		if (categoryClassInstance == null)
		{
			return;
		}
		string categoryName = GetCategoryName(categoryClassInstance);
		PropertyInfo[] properties = categoryClassInstance.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!propertyInfo.CanWrite)
			{
				continue;
			}
			string settingName = GetSettingName(propertyInfo);
			if (populateDefaultValues)
			{
				_defaultValues[(categoryName, settingName)] = propertyInfo.GetValue(categoryClassInstance, null);
			}
			bool flag = false;
			KioskSettingValue kioskSettingValue = _kioskConfigurationSettings.GetKioskSettingValue(categoryName, settingName);
			if (kioskSettingValue != null)
			{
				try
				{
					string text = kioskSettingValue.Value;
					if (kioskSettingValue.EncryptionType.HasValue)
					{
						text = _encryptionService.Decrypt(kioskSettingValue.EncryptionType.Value, text).Result;
					}
					object value = Convert.ChangeType(text, propertyInfo.PropertyType);
					propertyInfo.SetValue(categoryClassInstance, value, null);
				}
				catch (Exception ex)
				{
					flag = true;
					_logger.LogErrorWithSource(ex, "Unable to set " + categoryName + " property " + settingName, "PopulateCategorySettingValues");
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			try
			{
				if (_defaultValues.TryGetValue((categoryName, settingName), out var value2))
				{
					propertyInfo.SetValue(categoryClassInstance, value2, null);
				}
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Unable to set " + categoryName + " property " + settingName + " with default value.", "PopulateCategorySettingValues");
			}
		}
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
					_logger.LogErrorWithSource("Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name, "GetCategoryClassInstance");
				}
				SetParentKioskConfiguration(obj);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name, "GetCategoryClassInstance");
			}
		}
		return obj;
	}

	private void SetParentKioskConfiguration(object categoryClassInstance)
	{
		if (categoryClassInstance == null)
		{
			return;
		}
		PropertyInfo property = categoryClassInstance.GetType().GetProperty("ParentKioskConfiguration");
		if (property != null)
		{
			object value = property.GetValue(categoryClassInstance);
			if (value == null)
			{
				property.SetValue(categoryClassInstance, this);
			}
		}
	}
}
