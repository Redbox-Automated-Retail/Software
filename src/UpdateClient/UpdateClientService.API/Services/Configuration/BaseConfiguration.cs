using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Certificate.Security;

namespace UpdateClientService.API.Services.Configuration;

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
		string header = $"KioskConfiguration Settings for configuration version: {_kioskConfigurationSettings.ConfigurationVersion}";
		if (_kioskConfigurationSettings.ConfigurationVersion == 0L)
		{
			_logger.LogWarning(header + "  KioskConfiguration is not current.");
		}
		else
		{
			_logger.LogInformation(header);
		}
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
		string className = categoryClassInstance.GetType().Name;
		string propertyName = settingProperty.Name;
		string settingName = GetSettingName(settingProperty);
		bool noCategoryFilter = loggingParameters?.IncludeCategories == null || !loggingParameters.IncludeCategories.Any();
		bool categoryIncluded = loggingParameters?.IncludeCategories != null && loggingParameters.IncludeCategories.Intersect(new string[2] { categoryName, className }).Any();
		bool categoryExcluded = loggingParameters?.ExcludeCategories != null && loggingParameters.ExcludeCategories.Intersect(new string[2] { categoryName, className }).Any();
		bool settingIncluded = loggingParameters?.IncludeSettings != null && loggingParameters.IncludeSettings.Intersect(new(string, string)[4]
		{
			(categoryName, settingName),
			(categoryName, propertyName),
			(className, settingName),
			(className, propertyName)
		}).Any();
		bool settingExcluded = loggingParameters?.ExcludeSettings != null && loggingParameters.ExcludeSettings.Intersect(new(string, string)[4]
		{
			(categoryName, settingName),
			(categoryName, propertyName),
			(className, settingName),
			(className, propertyName)
		}).Any();
		if (!settingIncluded)
		{
			if ((noCategoryFilter && !categoryExcluded) | categoryIncluded)
			{
				return !settingExcluded;
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
			bool useDefault = false;
			KioskSettingValue kioskSettingValue = _kioskConfigurationSettings.GetKioskSettingValue(categoryName, settingName);
			if (kioskSettingValue != null)
			{
				try
				{
					string rawValue = kioskSettingValue.Value;
					bool convertValue = true;
					if (kioskSettingValue.EncryptionType.HasValue)
					{
						if (GetMaskLogValueAttribute(propertyInfo) != null)
						{
							rawValue = _encryptionService.Decrypt(kioskSettingValue.EncryptionType.Value, rawValue).Result;
						}
						else
						{
							convertValue = false;
							rawValue = "[Encrypted]";
						}
					}
					settingValue = (convertValue ? Convert.ChangeType(rawValue, propertyInfo.PropertyType) : rawValue);
				}
				catch (Exception ex)
				{
					useDefault = true;
					_logger.LogErrorWithSource(ex, "Unable to get " + categoryName + " property " + settingName, "LogCategorySettingValues", "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
				}
			}
			else
			{
				useDefault = true;
			}
			if (useDefault)
			{
				try
				{
					if (_defaultValues.TryGetValue((categoryName, settingName), out var defaultValue))
					{
						settingValue = defaultValue;
					}
				}
				catch (Exception ex2)
				{
					_logger.LogErrorWithSource(ex2, "Unable to get " + categoryName + " property " + settingName + " with default value.", "LogCategorySettingValues", "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
				}
			}
			settingValue = MaskSettingValue(propertyInfo, settingValue);
			string defaultSuffix = (useDefault ? " [Default value]" : null);
			_logger.LogInformation($"KioskConfiguration.{categoryName}.{settingName} = {settingValue}{defaultSuffix}");
		}
	}

	private string MaskSettingValue(PropertyInfo settingPropertyInfo, object settingValue)
	{
		string valueText = $"{settingValue}";
		MaskLogValue maskLogValueAttribute = GetMaskLogValueAttribute(settingPropertyInfo);
		if (maskLogValueAttribute != null && !string.IsNullOrEmpty(valueText))
		{
			int count = Math.Max(0, valueText.Length - maskLogValueAttribute.VisibleChars);
			int startIndex = Math.Max(0, valueText.Length - maskLogValueAttribute.VisibleChars);
			valueText = new string('X', count) + valueText.Substring(startIndex);
		}
		return valueText;
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
			bool populateDefaults = _defaultValues == null;
			if (populateDefaults)
			{
				_defaultValues = new Dictionary<(string, string), object>();
			}
			PropertyInfo[] properties = GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.PropertyType.IsSubclassOf(typeof(BaseCategorySetting)))
				{
					object categoryClassInstance = GetCategoryClassInstance(propertyInfo);
					PopulateCategorySettingValues(categoryClassInstance, populateDefaults);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while populating configuration setting values.", "PopulateConfigurationSettingValues", "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
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
			bool useDefault = false;
			KioskSettingValue kioskSettingValue = _kioskConfigurationSettings.GetKioskSettingValue(categoryName, settingName);
			if (kioskSettingValue != null)
			{
				try
				{
					string rawValue = kioskSettingValue.Value;
					if (kioskSettingValue.EncryptionType.HasValue)
					{
						rawValue = _encryptionService.Decrypt(kioskSettingValue.EncryptionType.Value, rawValue).Result;
					}
					object convertedValue = Convert.ChangeType(rawValue, propertyInfo.PropertyType);
					propertyInfo.SetValue(categoryClassInstance, convertedValue, null);
				}
				catch (Exception ex)
				{
					useDefault = true;
					_logger.LogErrorWithSource(ex, "Unable to set " + categoryName + " property " + settingName, "PopulateCategorySettingValues", "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
				}
			}
			else
			{
				useDefault = true;
			}
			if (!useDefault)
			{
				continue;
			}
			try
			{
				if (_defaultValues.TryGetValue((categoryName, settingName), out var defaultValue))
				{
					propertyInfo.SetValue(categoryClassInstance, defaultValue, null);
				}
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Unable to set " + categoryName + " property " + settingName + " with default value.", "PopulateCategorySettingValues", "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
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
		object categoryInstance = categoryClassPropertyInfo?.GetValue(this, null);
		if (categoryInstance == null)
		{
			try
			{
				categoryInstance = Activator.CreateInstance(categoryClassPropertyInfo.PropertyType);
				if (categoryInstance != null)
				{
					categoryClassPropertyInfo.SetValue(this, categoryInstance);
				}
				else
				{
					_logger.LogErrorWithSource("Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name, "GetCategoryClassInstance", "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
				}
				SetParentKioskConfiguration(categoryInstance);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name, "GetCategoryClassInstance", "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
			}
		}
		return categoryInstance;
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
