using System;
using System.Collections.Generic;
using System.Formats.Nrbf;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Redbox.HAL.Script.Framework;

if (args.Length != 1 || !Directory.Exists(args[0]))
{
	Console.Error.WriteLine("usage: HalContextStore.Check <contexts folder>");
	return 2;
}
string work = Path.Combine(Path.GetTempPath(), "HalContextStore.Check-" + Guid.NewGuid().ToString("N"));
CopyDirectory(args[0], work);
int checkedCount = 0;
List<string> failures = new List<string>();
try
{
	foreach (string directory in Directory.GetDirectories(work).OrderBy(d => d, StringComparer.Ordinal))
	{
		string id = Path.GetFileName(directory);
		string legacyPath = Path.Combine(directory, "ctx.data");
		if (!File.Exists(legacyPath))
		{
			continue;
		}
		Dictionary<string, string?> expected = DecodeLegacy(legacyPath);
		ExecutionContext imported = ExecutionContextStore.Load(directory);
		Dictionary<string, string?> first = Entries(imported);
		if (!File.Exists(Path.Combine(directory, ExecutionContextStore.FileName)) || File.Exists(legacyPath))
		{
			failures.Add($"{id}: import did not leave ctx.json in place of ctx.data");
		}
		Dictionary<string, string?> second = Entries(ExecutionContextStore.Load(directory));
		Compare($"{id} BinaryFormatter file vs imported", expected, first, failures);
		Compare($"{id} imported vs reloaded from ctx.json", first, second, failures);
		checkedCount++;
		Console.WriteLine($"{id}: {imported.ProgramName}, {first.Count} entries");
	}
}
finally
{
	Directory.Delete(work, recursive: true);
}
foreach (string failure in failures)
{
	Console.WriteLine("FAIL " + failure);
}
Console.WriteLine(failures.Count == 0 && checkedCount > 0 ? $"ALL PASSED ({checkedCount} contexts)" : $"{failures.Count} failures, {checkedCount} contexts");
return failures.Count == 0 && checkedCount > 0 ? 0 : 1;

// The context as GetObjectData describes it, every value as invariant text.
static Dictionary<string, string?> Entries(ExecutionContext context)
{
	SerializationInfo info = new SerializationInfo(typeof(ExecutionContext), new FormatterConverter());
	((ISerializable)context).GetObjectData(info, new StreamingContext(StreamingContextStates.Persistence));
	Dictionary<string, string?> entries = new Dictionary<string, string?>();
	foreach (SerializationEntry entry in info)
	{
		if (entry.Value is IDictionary<string, object> symbols)
		{
			foreach (KeyValuePair<string, object> symbol in symbols)
			{
				entries["SymbolTable." + symbol.Key] = Text(symbol.Value);
			}
		}
		else
		{
			entries[entry.Name] = Text(entry.Value);
		}
	}
	return entries;
}

// ctx.data decoded without Redbox.HAL code: the members of the ExecutionContext record.
static Dictionary<string, string?> DecodeLegacy(string path)
{
	using FileStream stream = File.OpenRead(path);
	ClassRecord record = NrbfDecoder.DecodeClassRecord(stream);
	Dictionary<string, string?> entries = new Dictionary<string, string?>();
	foreach (string name in record.MemberNames)
	{
		object? value = record.GetRawValue(name);
		if (value is ClassRecord member && name == "SymbolTable")
		{
			if (member.GetRawValue("KeyValuePairs") is SZArrayRecord<SerializationRecord> pairs)
			{
				foreach (ClassRecord pair in pairs.GetArray().OfType<ClassRecord>())
				{
					object? symbol = pair.GetRawValue("value");
					entries["SymbolTable." + pair.GetString("key")] = symbol is ClassRecord boxed ? Convert.ToString(boxed.GetRawValue("value__"), CultureInfo.InvariantCulture) : Text(symbol);
				}
			}
		}
		else if (value is ClassRecord builder)
		{
			entries[name] = builder.GetString("m_StringValue") ?? string.Empty;
		}
		else
		{
			entries[name] = Text(value);
		}
	}
	return entries;
}

static string? Text(object? value)
{
	return value switch
	{
		null => "<null>",
		DateTime time => time.ToString("O", CultureInfo.InvariantCulture),
		Enum e => Convert.ToString(Convert.ChangeType(e, Enum.GetUnderlyingType(e.GetType())), CultureInfo.InvariantCulture),
		_ => Convert.ToString(value, CultureInfo.InvariantCulture)
	};
}

static void Compare(string what, Dictionary<string, string?> expected, Dictionary<string, string?> actual, List<string> failures)
{
	foreach (string key in expected.Keys.Union(actual.Keys).OrderBy(k => k, StringComparer.Ordinal))
	{
		expected.TryGetValue(key, out string? e);
		actual.TryGetValue(key, out string? a);
		if (key == "Errors" && e != null && a != null)
		{
			// HAL's Error parsing keeps the space after "ERROR:", so each load adds one (the .NET Framework HAL as well).
			e = Regex.Replace(e, " {2,}", " ");
			a = Regex.Replace(a, " {2,}", " ");
		}
		if (e != a)
		{
			failures.Add($"{what}: {key}: expected '{e ?? "<missing>"}', got '{a ?? "<missing>"}'");
		}
	}
}

static void CopyDirectory(string source, string target)
{
	Directory.CreateDirectory(target);
	foreach (string file in Directory.GetFiles(source))
	{
		File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
	}
	foreach (string directory in Directory.GetDirectories(source))
	{
		CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
	}
}