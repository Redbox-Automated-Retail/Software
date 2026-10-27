using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class InventoryTable : AbstractVistaDBDataTable<ILocation>
{
	protected override string PrimaryKeyToken => "inventory_PK";

	protected override string PrimaryKeyColumns => "( Deck, Slot )";

	protected override List<ILocation> OnLoadEntries()
	{
		List<ILocation> list = new List<ILocation>();
		string query = $"SELECT Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags, Deck FROM [{base.Name}] ";
		ExectuteSelectQuery(delegate(DbDataReader reader)
		{
			DateTime? returnDate = null;
			if (!reader.IsDBNull(2))
			{
				returnDate = reader.GetDateTime(2);
			}
			list.Add(new Location(reader.GetInt32(6), reader.GetInt32(0))
			{
				ID = reader.GetString(1),
				ReturnDate = returnDate,
				Excluded = reader.GetBoolean(3),
				StuckCount = reader.GetInt32(4),
				Flags = (MerchFlags)reader.GetInt64(5)
			});
		}, query);
		return list;
	}

	protected override string UpdateStatement(ILocation l)
	{
		return $"UPDATE {base.Name} SET ID = '{l.ID}', LastReturnTime = {FormatReturnTime(l)}, Excluded = '{(l.Excluded ? 1 : 0)}', EmptyStuckCount = '{l.StuckCount}', MerchFlags = '{(int)l.Flags}' WHERE  Deck = '{l.Deck}' AND Slot = '{l.Slot}'";
	}

	protected override string InsertStatement(ILocation l)
	{
		return $"INSERT INTO [{base.Name}] (Deck, Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags) VALUES ('{l.Deck}', '{l.Slot}', '{l.ID}', {FormatReturnTime(l)}, '{(l.Excluded ? 1 : 0)}', '{l.StuckCount}', '{(int)l.Flags}')";
	}

	protected override string DeleteStatement(ILocation obj)
	{
		return $"delete from {base.Name} where Deck = '{obj.Deck}' and Slot = '{obj.Slot}'";
	}

	protected override string CreateStatement()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("CREATE TABLE [{0}] ", base.Name);
		stringBuilder.Append("( [Deck] Int NOT NULL,");
		stringBuilder.Append(" [Slot] Int NOT NULL,");
		stringBuilder.Append(" [ID] VarChar(100) NOT NULL,");
		stringBuilder.Append(" [LastReturnTime] DateTime,");
		stringBuilder.Append(" [Excluded] Bit NOT NULL,");
		stringBuilder.Append(" [EmptyStuckCount] Int NOT NULL,");
		stringBuilder.Append(" [MerchFlags] Int NOT NULL,");
		stringBuilder.AppendFormat(" CONSTRAINT {0} PRIMARY KEY {1} )", PrimaryKeyToken, PrimaryKeyColumns);
		return stringBuilder.ToString();
	}

	internal InventoryTable(IDataTableDescriptor descriptor)
		: base(descriptor, "Inventory_v1")
	{
	}

	private string FormatReturnTime(ILocation location)
	{
		if (location.ReturnDate.HasValue)
		{
			return $"'{location.ReturnDate.Value}'";
		}
		return "NULL";
	}
}
