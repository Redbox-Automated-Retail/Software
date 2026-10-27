using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal class PreferencePage : IPreferencePage
{
	private IPreferencePageHost _preferencePageHost;

	public string Name { get; internal set; }

	public string SettingPath { get; internal set; }

	public string DisplayPath { get; internal set; }

	public IPreferencePageHost Host
	{
		get
		{
			if (_preferencePageHost == null)
			{
				_preferencePageHost = GetPreferencePageHost?.Invoke();
			}
			return _preferencePageHost;
		}
		internal set
		{
			_preferencePageHost = value;
		}
	}

	public Func<IPreferencePageHost> GetPreferencePageHost { get; set; }

	public PreferencePageTarget Target { get; internal set; }

	public event EventHandler Activate;

	public event EventHandler Deactivate;

	public T GetValue<T>(string settingName)
	{
		return GetValue(settingName, default(T));
	}

	public T GetValue<T>(string settingName, T defaultValue)
	{
		if (Target != PreferencePageTarget.LocalSystem)
		{
			return UserSettingsStore.Instance.GetValue(SettingPath, settingName, defaultValue);
		}
		return MachineSettingsStore.Instance.GetValue(SettingPath, settingName, defaultValue);
	}

	public void SetValue(string settingName, object value)
	{
		if (Target == PreferencePageTarget.LocalSystem)
		{
			MachineSettingsStore.Instance.SetValue(SettingPath, settingName, value);
		}
		else
		{
			UserSettingsStore.Instance.SetValue(SettingPath, settingName, value);
		}
	}

	public void RaiseActivate()
	{
		if (Activate != null)
		{
			Activate(this, EventArgs.Empty);
		}
	}

	public void RaiseDeactivate()
	{
		if (Deactivate != null)
		{
			Deactivate(this, EventArgs.Empty);
		}
	}
}
