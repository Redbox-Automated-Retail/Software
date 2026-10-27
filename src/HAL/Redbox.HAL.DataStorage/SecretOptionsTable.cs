using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class SecretOptionsTable : AbstractVistaDBDataTable<IPersistentOption>
{
	protected override string PrimaryKeyToken => "secretOptions_PK";

	protected override string PrimaryKeyColumns => "( OptionName )";

	protected override List<IPersistentOption> OnLoadEntries()
	{
		List<IPersistentOption> options = new List<IPersistentOption>();
		ExectuteSelectQuery(delegate(DbDataReader reader)
		{
			options.Add(new PersistentOption
			{
				Key = reader.GetString(0),
				Value = reader.GetString(1)
			});
		}, $"SELECT OptionName, OptionValue, OptionName FROM [{base.Name}]");
		return options;
	}

	protected override string UpdateStatement(IPersistentOption obj)
	{
		return $"UPDATE [{base.Name}] SET OptionValue = '{obj.Value}' WHERE OptionName = '{obj.Key}'";
	}

	protected override string InsertStatement(IPersistentOption obj)
	{
		return $"INSERT INTO [{base.Name}] ( OptionName, OptionValue, OptionEncrypted ) VALUES( '{obj.Key}','{obj.Value}', '0' )";
	}

	protected override string DeleteStatement(IPersistentOption obj)
	{
		return $"DELETE FROM [{base.Name}] WHERE OptionName = '{obj.Key}' ";
	}

	protected override string CreateStatement()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("CREATE TABLE [{0}] ", base.Name);
		stringBuilder.Append("( [OptionName] VarChar(32) NOT NULL,");
		stringBuilder.Append(" [OptionValue] VarChar(64) NOT NULL,");
		stringBuilder.Append(" [OptionEncrypted] Bit NOT NULL,");
		stringBuilder.AppendFormat(" CONSTRAINT {0} PRIMARY KEY {1} )", PrimaryKeyToken, PrimaryKeyColumns);
		return stringBuilder.ToString();
	}

	internal SecretOptionsTable(IDataTableDescriptor d)
		: base(d, "SecretOptions_v2")
	{
	}
}
