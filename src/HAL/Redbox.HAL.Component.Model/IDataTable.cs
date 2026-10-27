using System.Collections.Generic;
using System.IO;

namespace Redbox.HAL.Component.Model;

public interface IDataTable<T>
{
	bool Exists { get; }

	string Name { get; }

	List<T> LoadEntries(bool debug);

	List<T> LoadEntries();

	bool Update(T item);

	bool Update(IList<T> entries);

	bool Insert(T p);

	bool Insert(IList<T> entries);

	bool Delete(T item);

	bool Delete(IList<T> items);

	int DeleteAll();

	bool Create();

	bool Drop();

	int Clean();

	int GetRowCount();

	void UpdateTable(TextWriter writer, ErrorList errors);

	void ExectuteSelectQuery(OnSelectResult callback, string query);

	bool Recreate(ErrorList errors);
}
