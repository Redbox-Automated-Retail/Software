using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IDataFileStoreService
{
	string Root { get; }

	void Initialize(string dataStoreName, string path, string extension, string corruptFileExtension, bool useEncryption);

	void Reset();

	bool Set(string fileName, object o);

	bool SetRaw(Guid guidFileName, string o);

	bool SetRaw(string fileName, string o);

	bool Get<T>(string fileName, out T deserializeObject);

	List<T> GetAll<T>();

	string GetRaw(Guid fileName);

	string GetRaw(string fileName);

	void Delete(string fileName);
}
