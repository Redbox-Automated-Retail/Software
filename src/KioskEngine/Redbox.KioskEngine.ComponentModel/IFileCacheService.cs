namespace Redbox.KioskEngine.ComponentModel;

public interface IFileCacheService
{
	void Remove(string pattern);

	void FlushCache();

	string GetCachePath();

	void SetFileContent(string fileName, byte[] data, bool overwrite);

	void SetFileContent(string fileName, string data, bool overwrite);

	byte[] GetFileContent(string fileName);

	string GetFileContentAsString(string fileName);
}
