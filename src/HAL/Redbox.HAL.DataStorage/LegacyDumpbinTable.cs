using System;
using System.Collections.Generic;
using System.Data.Common;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class LegacyDumpbinTable : AbstractVistaDBDataTable<IDumpBinInventoryItem>
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

	protected override string UpdateStatement(IDumpBinInventoryItem obj)
	{
		throw new NotSupportedException("Update on legacy table not supported.");
	}

	protected override string InsertStatement(IDumpBinInventoryItem item)
	{
		throw new NotSupportedException("Insert on legacy table not supported.");
	}

	protected override string DeleteStatement(IDumpBinInventoryItem obj)
	{
		throw new NotSupportedException("Delete on legacy table not supported.");
	}

	protected override string CreateStatement()
	{
		throw new NotSupportedException("Create on legacy table not supported.");
	}

	internal LegacyDumpbinTable(IDataTableDescriptor d)
		: base(d, "BarcodesInBin")
	{
	}
}
