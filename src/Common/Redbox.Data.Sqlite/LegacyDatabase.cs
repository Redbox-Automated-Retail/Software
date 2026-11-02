using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Redbox.Data.Sqlite;

public enum LegacyDatabaseKind
{
	// The component writes to it (message queue, data stores, HAL tables). Converted once; the VistaDB file is then
	// renamed to "<name>.vistadb-backup" and the SQLite file is the database from then on.
	Writable,

	// Delivered and replaced from outside (profile.data, by the update store). The VistaDB file stays where it is and is
	// converted again whenever it changes.
	ReadOnlySource
}

// Maps a VistaDB data file used by the .NET Framework builds (e.g. data\Local.data, halservice\bin\data\HALData.vdb3)
// to the SQLite file the .NET 10 builds use (same folder and name, extension ".sqlite"), and converts the VistaDB file
// when needed with vistadb-to-sqlite.exe (.NET Framework 4.8, which is part of Windows 10/11; VistaDB 3.5 runs only
// there).
public static class LegacyDatabase
{
	public const string ConverterFileName = "vistadb-to-sqlite.exe";

	public const string ConverterEnvironmentVariable = "REDBOX_VISTADB_TO_SQLITE";

	public const string BackupSuffix = ".vistadb-backup";

	private static readonly object s_lock = new object();

	public static string ToSqlitePath(string legacyPath)
	{
		return Path.ChangeExtension(legacyPath, ".sqlite");
	}

	// VistaDB 3 files start with 24 00 00 00 05 00 00 00 (all the kiosk's .data/.vdb3 files do); SQLite files with
	// "SQLite format 3". Product lookup archives (inventory.data) are neither.
	public static bool IsVistaDb(string path)
	{
		try
		{
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			byte[] header = new byte[8];
			if (stream.Read(header, 0, header.Length) != header.Length)
			{
				return false;
			}
			return header[0] == 0x24 && header[1] == 0 && header[2] == 0 && header[3] == 0 && header[4] == 0x05 && header[5] == 0 && header[6] == 0 && header[7] == 0;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	// Returns the SQLite path for legacyPath, converting the VistaDB file first when needed. Throws when a conversion
	// is needed and fails, so that callers never start an empty database in place of existing data.
	public static string Resolve(string legacyPath, LegacyDatabaseKind kind, Action<string>? log = null)
	{
		string sqlitePath = ToSqlitePath(legacyPath);
		if (!File.Exists(legacyPath) || !IsVistaDb(legacyPath))
		{
			return sqlitePath;
		}
		lock (s_lock)
		{
			using Mutex mutex = CreateMutex(sqlitePath);
			bool owned = false;
			try
			{
				try
				{
					owned = mutex.WaitOne(TimeSpan.FromMinutes(30));
				}
				catch (AbandonedMutexException)
				{
					owned = true;
				}
				if (!File.Exists(legacyPath))
				{
					return sqlitePath;
				}
				if (kind == LegacyDatabaseKind.Writable)
				{
					if (File.Exists(sqlitePath))
					{
						// Both exist: an earlier conversion could not rename the VistaDB file, or a .NET Framework build ran
						// again. The SQLite file is the newer database; keep the VistaDB file untouched.
						log?.Invoke($"Both {Path.GetFileName(legacyPath)} (VistaDB) and {Path.GetFileName(sqlitePath)} exist; using {Path.GetFileName(sqlitePath)}.");
						return sqlitePath;
					}
					ConvertFile(legacyPath, sqlitePath, log);
					string backup = legacyPath + BackupSuffix;
					if (File.Exists(backup))
					{
						backup = legacyPath + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + BackupSuffix;
					}
					File.Move(legacyPath, backup);
					log?.Invoke($"Converted {Path.GetFileName(legacyPath)} to {Path.GetFileName(sqlitePath)}; the VistaDB file is kept as {Path.GetFileName(backup)}.");
					return sqlitePath;
				}
				ConversionStamp current = ConversionStamp.FromFile(legacyPath);
				if (current.Matches(ConversionStamp.Read(sqlitePath)))
				{
					return sqlitePath;
				}
				ConvertFile(legacyPath, sqlitePath, log);
				log?.Invoke($"Converted {Path.GetFileName(legacyPath)} to {Path.GetFileName(sqlitePath)}.");
				return sqlitePath;
			}
			finally
			{
				if (owned)
				{
					mutex.ReleaseMutex();
				}
			}
		}
	}

	public static string? FindConverter()
	{
		string? configured = Environment.GetEnvironmentVariable(ConverterEnvironmentVariable);
		if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
		{
			return configured;
		}
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		foreach (string candidate in new[]
		{
			Path.Combine(baseDirectory, "vistadb-to-sqlite", ConverterFileName),
			Path.Combine(baseDirectory, ConverterFileName)
		})
		{
			if (File.Exists(candidate))
			{
				return candidate;
			}
		}
		return null;
	}

	// Converts into a temporary file and moves it over sqlitePath only when the converter succeeded.
	private static void ConvertFile(string legacyPath, string sqlitePath, Action<string>? log)
	{
		string converter = FindConverter() ?? throw new InvalidOperationException($"{Path.GetFileName(legacyPath)} is a VistaDB database and has to be converted to SQLite, but {ConverterFileName} was not found (looked in {AppDomain.CurrentDomain.BaseDirectory} and its vistadb-to-sqlite folder, and the {ConverterEnvironmentVariable} environment variable).");
		string temp = sqlitePath + ".converting";
		if (File.Exists(temp))
		{
			File.Delete(temp);
		}
		log?.Invoke($"Converting {legacyPath} (VistaDB) to SQLite with {converter}...");
		ProcessStartInfo startInfo = new ProcessStartInfo(converter, $"\"{legacyPath}\" \"{temp}\"")
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		using Process process = Process.Start(startInfo);
		StringBuilder output = new StringBuilder();
		process.OutputDataReceived += (_, e) => { if (e.Data != null) { lock (output) { output.AppendLine(e.Data); } } };
		process.ErrorDataReceived += (_, e) => { if (e.Data != null) { lock (output) { output.AppendLine(e.Data); } } };
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		process.WaitForExit();
		log?.Invoke(output.ToString().TrimEnd());
		if (process.ExitCode != 0 || !File.Exists(temp))
		{
			if (File.Exists(temp))
			{
				File.Delete(temp);
			}
			throw new InvalidOperationException($"{ConverterFileName} failed with exit code {process.ExitCode} converting {legacyPath}.");
		}
		if (File.Exists(sqlitePath))
		{
			File.Delete(sqlitePath);
		}
		File.Move(temp, sqlitePath);
	}

	private static Mutex CreateMutex(string sqlitePath)
	{
		string name;
		using (SHA1 sha = SHA1.Create())
		{
			name = "Redbox.VistaDbToSqlite." + BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(sqlitePath).ToUpperInvariant()))).Replace("-", string.Empty);
		}
		try
		{
			return new Mutex(initiallyOwned: false, "Global\\" + name);
		}
		catch (UnauthorizedAccessException)
		{
			return new Mutex(initiallyOwned: false, "Local\\" + name);
		}
	}
}
