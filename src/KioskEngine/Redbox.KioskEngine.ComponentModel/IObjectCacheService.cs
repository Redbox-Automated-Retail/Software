namespace Redbox.KioskEngine.ComponentModel;

public interface IObjectCacheService
{
	T GetObject<T>(string name);

	void SetObject<T>(string name, T value);
}
