namespace Redbox.HAL.Component.Model;

public interface IDataTableService
{
	IDataTable<T> GetTable<T>();

	IDataTable<T> GetLegacyTable<T>();

	bool Add<T>(IDataTable<T> table);
}
