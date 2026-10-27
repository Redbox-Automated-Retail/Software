using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class PersistentCounterTable : AbstractVistaDBDataTable<IPersistentCounter>
{
	protected override string PrimaryKeyToken => "counters_PK";

	protected override string PrimaryKeyColumns => "( Name )";

	protected override List<IPersistentCounter> OnLoadEntries()
	{
		List<IPersistentCounter> list = new List<IPersistentCounter>();
		string query = $"SELECT Name, Value FROM [{base.Name}]";
		ExectuteSelectQuery(delegate(DbDataReader reader)
		{
			list.Add(new PersistentCounter
			{
				Name = reader.GetString(0),
				Value = reader.GetInt32(1)
			});
		}, query);
		return list;
	}

	protected override string UpdateStatement(IPersistentCounter item)
	{
		return $"UPDATE {base.Name} SET VALUE = '{item.Value}' WHERE NAME = '{item.Name}'";
	}

	protected override string InsertStatement(IPersistentCounter item)
	{
		return $"INSERT INTO [{base.Name}] (Name, Value) VALUES ('{item.Name}', '{item.Value}')";
	}

	protected override string DeleteStatement(IPersistentCounter obj)
	{
		return $"delete from {base.Name} where Name = '{obj.Name}'";
	}

	protected override string CreateStatement()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("CREATE TABLE [{0}] ", base.Name);
		stringBuilder.Append("( [Name] VarChar(100) NOT NULL,");
		stringBuilder.Append(" [Value] Int NOT NULL,");
		stringBuilder.Append(" [LastUpdateTime] DateTime,");
		stringBuilder.AppendFormat(" CONSTRAINT {0} PRIMARY KEY {1} )", PrimaryKeyToken, PrimaryKeyColumns);
		return stringBuilder.ToString();
	}

	internal PersistentCounterTable(IDataTableDescriptor d)
		: base(d, "Counters")
	{
	}
}
