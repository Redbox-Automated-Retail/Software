namespace Redbox.KioskEngine.ComponentModel;

public interface IDataCacheService
{
	void Shutdown();

	byte[] GetContent(DataCacheType type, string name);
}
