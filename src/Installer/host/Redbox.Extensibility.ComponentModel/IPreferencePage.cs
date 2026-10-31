using System;

namespace Redbox.Extensibility.ComponentModel;

public interface IPreferencePage
{
	string Name { get; }

	string SettingPath { get; }

	string DisplayPath { get; }

	IPreferencePageHost Host { get; }

	PreferencePageTarget Target { get; }

	event EventHandler Activate;

	event EventHandler Deactivate;

	T GetValue<T>(string settingName);

	void SetValue(string settingName, object value);

	void RaiseActivate();

	void RaiseDeactivate();
}
