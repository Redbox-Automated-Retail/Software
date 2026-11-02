using System.Data.SqlClient;

namespace Redbox.TimeKeeper;

public interface IDatabase
{
	string? ConnectionString { get; }

	long Modify(SqlCommand command);
}
