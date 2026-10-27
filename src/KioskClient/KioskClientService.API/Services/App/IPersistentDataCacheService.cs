using System.Threading.Tasks;

namespace KioskClientService.API.Services.App;

public interface IPersistentDataCacheService
{
	Task<PersistentDataWrapper<T>> Read<T>(string fileName, bool useCache = false, bool log = true) where T : IPersistentData;

	Task<bool> Write<T>(T persistentData, string fileName) where T : IPersistentData;

	bool Delete(string fileName);
}
