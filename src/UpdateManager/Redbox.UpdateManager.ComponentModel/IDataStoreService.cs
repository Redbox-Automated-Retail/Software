using System;

namespace Redbox.UpdateManager.ComponentModel;

public interface IDataStoreService
{
	void Set(Guid id, object o);

	void Set(string id, object o);

	void SetRaw(Guid id, string o);

	void SetRaw(string id, string o);

	T Get<T>(Guid id);

	T Get<T>(string id);

	string GetRaw(Guid id);

	string GetRaw(string id);

	void Delete(Guid id);

	void Delete(string id);

	void Reset();

	void CleanUp();

	void CleanUp(string extension);
}
