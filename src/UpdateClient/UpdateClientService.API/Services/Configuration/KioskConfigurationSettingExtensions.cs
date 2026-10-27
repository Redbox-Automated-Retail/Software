using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.Services.IoT.Certificate.Security;

namespace UpdateClientService.API.Services.Configuration;

public static class KioskConfigurationSettingExtensions
{
	public static T GetConfigValue<T>(this KioskConfigurationSettings kioskConfigurationSettings, string categoryName, string settingName, T defaultValue, ILogger logger, IEncryptionService encryptionService)
	{
		T result = defaultValue;
		try
		{
			KioskSettingValue kioskSettingValue = InnerGetKioskSettingValue(kioskConfigurationSettings, categoryName, settingName);
			if (kioskSettingValue != null)
			{
				string text = kioskSettingValue.Value;
				if (kioskSettingValue.EncryptionType.HasValue)
				{
					text = encryptionService.Decrypt(kioskSettingValue.EncryptionType.Value, text).Result;
				}
				result = (T)Convert.ChangeType(text, typeof(T));
			}
		}
		catch (Exception exception)
		{
			logger?.LogError(exception, "Unable to GetConfigValue for category " + categoryName + " and setting " + settingName);
		}
		return result;
	}

	public static KioskSettingValue GetKioskSettingValue(this KioskConfigurationSettings kioskConfigurationSettings, string categoryName, string settingName)
	{
		return InnerGetKioskSettingValue(kioskConfigurationSettings, categoryName, settingName);
	}

	private static KioskSettingValue InnerGetKioskSettingValue(KioskConfigurationSettings kioskConfigurationSettings, string categoryName, string settingName)
	{
		KioskSettingValue result = null;
		KioskSetting kioskSetting = kioskConfigurationSettings?.Categories?.FirstOrDefault((KioskConfigurationCategory x) => x.Name.Equals(categoryName, StringComparison.CurrentCultureIgnoreCase))?.Settings.FirstOrDefault((KioskSetting y) => y.Name.Equals(settingName, StringComparison.CurrentCultureIgnoreCase));
		if (kioskSetting != null)
		{
			result = (from x in kioskSetting.SettingValues
				where x.EffectiveDateTime <= DateTime.Now && (!x.ExpireDateTime.HasValue || x.ExpireDateTime > DateTime.Now)
				orderby x.Rank descending, x.EffectiveDateTime descending
				select x).FirstOrDefault();
		}
		return result;
	}
}
