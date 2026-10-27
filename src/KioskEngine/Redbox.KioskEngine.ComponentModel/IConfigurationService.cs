using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IConfigurationService
{
	ErrorList Start();

	ErrorList Stop();

	ErrorList Add(string key, string filepath, string defaultCategory);

	ErrorList Add(string key, string filepath, string defaultCategory, TimeSpan? checkTime, Func<bool> canActivate, Action activateAction);

	ErrorList Remove(string key);

	ErrorList RemoveAll();

	List<ConfigurationClientItem> GetItems();

	bool TryGetValue<T>(string key, string setting, out T value);

	bool TryGetValue<T>(string key, string category, string setting, out T value);

	bool TryGetTimeSpanRangeValue(string key, string category, string setting, out TimeSpan? startTime, out TimeSpan? endTime);

	bool TryGetObject(string key, string setting, out object value);

	bool TryGetObject(string key, string category, string setting, out object value);

	bool TryGetConfigurationData(string key, string setting, out ConfigurationData value);

	bool TryGetConfigurationData(string key, string category, string setting, out ConfigurationData value);

	bool GetAllConfigurationData(string key, out List<ConfigurationData> list);

	bool GetAllConfigurationData(string key, string setting, out List<ConfigurationData> list);

	bool GetAllConfigurationData(string key, string category, string setting, out List<ConfigurationData> list);

	bool GetCategoryNames(string key, out List<string> list);

	bool GetSettingNames(string key, string category, out List<string> list);

	bool GetSettingNames(string key, out List<string> list);
}
