using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.Core;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment;

public static class SchemaMigrator
{
	public static void Migrate(string path)
	{
		if (!File.Exists(path))
		{
			LogHelper.Instance.Log("...Create database file: {0}", path);
			using VistaDBConnection connection = new VistaDBConnection();
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = connection;
			vistaDBCommand.CommandText = $"CREATE DATABASE '{path}', PAGE SIZE 1, LCID 1033, CASE SENSITIVE FALSE;";
			vistaDBCommand.ExecuteNonQuery();
		}
		int? schemaVersion = null;
		using VistaDBConnection vistaDBConnection = new VistaDBConnection($"Data Source={path};Open Mode=ExclusiveReadWrite;");
		vistaDBConnection.Open();
		using (VistaDBCommand vistaDBCommand2 = new VistaDBCommand())
		{
			vistaDBCommand2.Connection = vistaDBConnection;
			vistaDBCommand2.CommandText = "SELECT COUNT(*) FROM [database schema] WHERE typeid = 1 AND name = @name";
			vistaDBCommand2.Parameters.Add("@name", "System");
			if ((int)vistaDBCommand2.ExecuteScalar() == 0)
			{
				LogHelper.Instance.Log("...System table does not exist, create and run all migrations.");
				using VistaDBCommand vistaDBCommand3 = new VistaDBCommand();
				vistaDBCommand3.Connection = vistaDBConnection;
				vistaDBCommand3.CommandText = "\r\n                                CREATE TABLE System( [Key] varchar(16) NOT NULL PRIMARY KEY, Value varchar(256) );\r\n                                INSERT INTO System([Key],Value) VALUES( 'SchemaVersion', '-1' );";
				vistaDBCommand3.ExecuteNonQuery();
				schemaVersion = -1;
			}
		}
		if (!schemaVersion.HasValue)
		{
			using VistaDBCommand vistaDBCommand4 = new VistaDBCommand();
			vistaDBCommand4.Connection = vistaDBConnection;
			vistaDBCommand4.CommandText = "SELECT Value FROM System WHERE [Key]='SchemaVersion'";
			schemaVersion = Convert.ToInt32(vistaDBCommand4.ExecuteScalar());
			LogHelper.Instance.Log("...SchemaVersion is: {0}", schemaVersion);
		}
		Assembly assembly = typeof(SchemaMigrator).Assembly;
		string namespaceName = typeof(SchemaMigrator).Namespace;
		string fileName = Path.GetFileName(path);
		string resourcePrefix = $"{namespaceName}.Migrations.{fileName}";
		List<string> migrationScripts = new List<string>();
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string resourceName in manifestResourceNames)
		{
			if (resourceName.StartsWith(resourcePrefix))
			{
				migrationScripts.Add(resourceName);
			}
		}
		migrationScripts.Sort();
		if (schemaVersion == migrationScripts.Count)
		{
			LogHelper.Instance.Log("...Schema is up-to-date.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("...Migrate schema to current version.");
			if (schemaVersion != -1)
			{
				schemaVersion--;
			}
			for (int j = schemaVersion.Value + 1; j < migrationScripts.Count; j++)
			{
				string scriptName = migrationScripts[j];
				LogHelper.Instance.Log("...Executing migration script: {0}", scriptName.Replace(resourcePrefix + ".", string.Empty));
				using Stream stream = assembly.GetManifestResourceStream(scriptName);
				using VistaDBCommand vistaDBCommand5 = new VistaDBCommand();
				vistaDBCommand5.Connection = vistaDBConnection;
				vistaDBCommand5.CommandText = Encoding.ASCII.GetString(stream.GetBytes());
				vistaDBCommand5.ExecuteNonQuery();
			}
			using VistaDBCommand vistaDBCommand6 = new VistaDBCommand();
			vistaDBCommand6.Connection = vistaDBConnection;
			vistaDBCommand6.CommandText = "UPDATE System SET Value = @Value WHERE [Key] = @Key";
			vistaDBCommand6.Parameters.Add("@Key", "SchemaVersion");
			vistaDBCommand6.Parameters.Add("@Value", migrationScripts.Count);
			vistaDBCommand6.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised from script migration.", e);
		}
	}
}
