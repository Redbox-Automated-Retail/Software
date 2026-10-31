using System.Collections.ObjectModel;

namespace Redbox.Extensibility.ComponentModel;

public interface IPreferenceService
{
	ReadOnlyCollection<IPreferencePage> PreferencePages { get; }

	T GetValue<T>(string pageName, string settingName);

	void SetValue(string pageName, string settingName, object value);

	void RemoveAll();

	IPreferencePage AddPreferencePage(string name, string settingPath, string displayPath, PreferencePageTarget target, IPreferencePageHost host);

	bool RemovePreferencePage(string pageName);

	IPreferencePage GetPreferencePage(string pageName);
}
