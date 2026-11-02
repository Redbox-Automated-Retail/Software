using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Data.Sqlite;
using VistaDB;
using VistaDB.DDA;

namespace Redbox.Tools.VistaDbToSqlite;

// A VistaDB table's columns, identity and indexes, and the SQLite DDL for them.
internal sealed class TableShape
{
	internal sealed class Column
	{
		public string Name = null!;

		public VistaDBType Type;

		public bool AllowNull;
	}

	internal sealed class Index
	{
		public string Name = null!;

		public List<KeyValuePair<string, bool>> Columns = null!;

		public bool Primary;

		public bool Unique;
	}

	// Name and Columns above, and the table name, are always set by From.
	private string m_tableName = null!;

	private bool m_caseSensitive;

	private string? m_identityColumn;

	public List<Column> Columns { get; } = new List<Column>();

	public string? IdentityColumn => m_identityColumn;

	public string? IdentitySeed { get; private set; }

	public string? IdentityStep { get; private set; }

	public List<Index> Indexes { get; } = new List<Index>();

	public static TableShape From(IVistaDBTableSchema schema, bool caseSensitive)
	{
		TableShape shape = new TableShape
		{
			m_tableName = schema.Name,
			m_caseSensitive = caseSensitive
		};
		for (int i = 0; i < schema.ColumnCount; i++)
		{
			IVistaDBColumnAttributes column = schema[i];
			shape.Columns.Add(new Column
			{
				Name = column.Name,
				Type = column.Type,
				AllowNull = column.AllowNull
			});
		}
		foreach (IVistaDBIdentityInformation identity in schema.Identities.Values)
		{
			shape.m_identityColumn = identity.ColumnName;
			shape.IdentitySeed = identity.SeedValue;
			shape.IdentityStep = identity.StepExpression;
		}
		foreach (IVistaDBIndexInformation index in schema.Indexes.Values)
		{
			if (index.FTS || index.FKConstraint || index.Temporary)
			{
				continue;
			}
			shape.Indexes.Add(new Index
			{
				Name = index.Name,
				Columns = index.KeyStructure.Select(k => new KeyValuePair<string, bool>(schema[k.RowIndex].Name, k.Descending)).ToList(),
				Primary = index.Primary,
				Unique = index.Unique || index.Primary
			});
		}
		return shape;
	}

	// The identity column, when it is the whole primary key and an integer: it becomes INTEGER PRIMARY KEY AUTOINCREMENT.
	public string? IdentityKeyColumn
	{
		get
		{
			Index? primary = Indexes.FirstOrDefault(i => i.Primary);
			bool identityKey = primary != null && primary.Columns.Count == 1 && m_identityColumn != null && string.Equals(primary.Columns[0].Key, m_identityColumn, StringComparison.OrdinalIgnoreCase) && IsInteger(Columns.First(c => c.Name == primary.Columns[0].Key).Type);
			return identityKey ? m_identityColumn : null;
		}
	}

	public string CreateTableSql()
	{
		Index primary = Indexes.FirstOrDefault(i => i.Primary);
		bool identityKey = IdentityKeyColumn != null;
		List<string> definitions = new List<string>();
		foreach (Column column in Columns)
		{
			if (identityKey && string.Equals(column.Name, m_identityColumn, StringComparison.OrdinalIgnoreCase))
			{
				definitions.Add($"[{column.Name}] INTEGER PRIMARY KEY AUTOINCREMENT");
				continue;
			}
			definitions.Add($"[{column.Name}] {SqliteType(column.Type)}{(column.AllowNull ? string.Empty : " NOT NULL")}");
		}
		if (primary != null && !identityKey)
		{
			definitions.Add($"CONSTRAINT [{primary.Name}] PRIMARY KEY ( {string.Join(", ", primary.Columns.Select(c => $"[{c.Key}]"))} )");
		}
		return $"CREATE TABLE [{m_tableName}] ( {string.Join(", ", definitions)} )";
	}

	// SQLite index names are per database, VistaDB's per table (every data store table has an IX_Key), so the
	// SQLite name is "<table>_<index>".
	public IEnumerable<string> CreateIndexSql(HashSet<string> usedNames)
	{
		foreach (Index index in Indexes.Where(i => !i.Primary))
		{
			string name = $"{m_tableName}_{index.Name}";
			for (int n = 2; !usedNames.Add(name); n++)
			{
				name = $"{m_tableName}_{index.Name}_{n}";
			}
			yield return $"CREATE {(index.Unique ? "UNIQUE " : string.Empty)}INDEX [{name}] ON [{m_tableName}] ( {string.Join(", ", index.Columns.Select(c => $"[{c.Key}]{(c.Value ? " DESC" : string.Empty)}"))} )";
		}
	}

	private string SqliteType(VistaDBType type)
	{
		switch (type)
		{
		case VistaDBType.Char:
		case VistaDBType.NChar:
		case VistaDBType.VarChar:
		case VistaDBType.NVarChar:
		case VistaDBType.Text:
		case VistaDBType.NText:
		case VistaDBType.UniqueIdentifier:
			return m_caseSensitive ? "TEXT" : SqliteValues.TextType;
		case VistaDBType.TinyInt:
		case VistaDBType.SmallInt:
		case VistaDBType.Int:
		case VistaDBType.BigInt:
		case VistaDBType.Bit:
			return "INTEGER";
		case VistaDBType.Real:
		case VistaDBType.Float:
			return "REAL";
		case VistaDBType.Decimal:
		case VistaDBType.Money:
		case VistaDBType.SmallMoney:
			return "TEXT";
		case VistaDBType.DateTime:
		case VistaDBType.SmallDateTime:
			// Declared DATETIME (NUMERIC affinity); the values are "yyyy-MM-dd HH:mm:ss" text, which SQLite keeps as text.
			return "DATETIME";
		case VistaDBType.Image:
		case VistaDBType.VarBinary:
		case VistaDBType.Timestamp:
			return "BLOB";
		default:
			return string.Empty;
		}
	}

	private static bool IsInteger(VistaDBType type)
	{
		return type == VistaDBType.TinyInt || type == VistaDBType.SmallInt || type == VistaDBType.Int || type == VistaDBType.BigInt;
	}
}
