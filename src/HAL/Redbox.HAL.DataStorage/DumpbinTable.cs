using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class DumpbinTable : AbstractVistaDBDataTable<IDumpBinInventoryItem>
{
	protected override List<IDumpBinInventoryItem> OnLoadEntries()
	{
		List<IDumpBinInventoryItem> rv = new List<IDumpBinInventoryItem>();
		ExectuteSelectQuery(delegate(DbDataReader reader)
		{
			rv.Add(new DumpbinItem(reader.GetString(0), reader.GetDateTime(1)));
		}, $"SELECT Barcode, PutTime FROM [{base.Name}]");
		return rv;
	}

	protected override string CreateStatement()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("CREATE TABLE [{0}] ", base.Name);
		stringBuilder.Append("( [Barcode] VarChar(100) NOT NULL,");
		stringBuilder.Append(" [PutTime] DateTime NOT NULL )");
		return stringBuilder.ToString();
	}

	protected override string UpdateStatement(IDumpBinInventoryItem obj)
	{
		return $"UPDATE {base.Name} SET PutTime = '{obj.PutTime.ToString()}' where Barcode = '{obj.ID}'";
	}

	protected override string InsertStatement(IDumpBinInventoryItem item)
	{
		return $"INSERT INTO [{base.Name}] (Barcode, PutTime) VALUES ('{item.ID}', '{item.PutTime.ToString()}')";
	}

	protected override string DeleteStatement(IDumpBinInventoryItem obj)
	{
		return $"delete from {base.Name} where Barcode = '{obj.ID}' and PutTime = '{obj.PutTime}'";
	}

	internal DumpbinTable(IDataTableDescriptor desc)
		: base(desc, "BarcodesInBin")
	{
	}
}
