namespace Redbox.HAL.Component.Model;

public interface IPersistentMap
{
	T GetValue<T>(string key, T def);

	void SetValue<T>(string key, T value);

	void Remove(string key);
}
