namespace Redbox.KioskEngine.ComponentModel;

public interface IUserSettingsStore
{
	T GetValue<T>(string path, string key);

	T GetValue<T>(string path, string key, T defaultValue);

	void SetValue<T>(string path, string key, T value);

	void RemoveKey(string path);

	void RemoveValue(string path, string key);
}
