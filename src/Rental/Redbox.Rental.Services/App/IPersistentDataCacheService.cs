namespace Redbox.Rental.Services.App;

public interface IPersistentDataCacheService
{
	PersistentDataWrapper<T> Read<T>(string fileName, bool useCache = false) where T : IPersistentData;

	bool Write<T>(T persistentData, string fileName) where T : IPersistentData;

	bool Delete(string fileName);

	bool Exists(string fileName);
}
