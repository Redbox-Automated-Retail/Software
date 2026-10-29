namespace Redbox.Extensibility.ComponentModel;

public interface IMachineSettingsStore
{
	T GetValue<T>(string path, string key);

	void SetValue<T>(string path, string key, T value);

	void RemoveKey(string path);

	void RemoveValue(string path, string key);
}
