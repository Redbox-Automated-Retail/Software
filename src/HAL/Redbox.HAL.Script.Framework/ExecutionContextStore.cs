// Written for the modernize branch (not decompiled). Compiled for .NET 10 only.
#if !NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Formats.Nrbf;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework;

// Where a job (ExecutionContext) is kept between service restarts. The .NET Framework HAL wrote ctx.data with the
// BinaryFormatter, plus one BinaryFormatter file per compiled program; .NET 10 has no BinaryFormatter. The context still
// describes itself through ISerializable (GetObjectData / the deserialization constructor), so what is kept is the same:
// those entries are written to ctx.json, and a program is kept as its script source (<program>.hs) and compiled again
// when the job is loaded. Compiling the same source gives the same instructions, so the saved call frames still fit.
// A context the .NET Framework HAL saved (ctx.data) is read once with System.Formats.Nrbf, which decodes the
// BinaryFormatter format without running it, and saved again as ctx.json (ctx.data is renamed to ctx.data.imported).
// Its programs were BinaryFormatter object graphs, which are not read; such a job gets the current script's source.
#pragma warning disable SYSLIB0050, SYSLIB0051 // Formatter-based serialization types: used only as the entry list.
internal static class ExecutionContextStore
{
	internal const string FileName = "ctx.json";

	private const string LegacyFileName = "ctx.data";

	private const string SourceExtension = ".hs";

	private const string SymbolTableEntry = "SymbolTable";

	// The source of a program as the engine compiled it from today's scripts (set by ExecutionEngine).
	internal static Func<string, byte[]> CurrentProgramSource;

	internal static void Save(ExecutionContext context, string directory)
	{
		SerializationInfo info = new SerializationInfo(typeof(ExecutionContext), new FormatterConverter());
		((ISerializable)context).GetObjectData(info, new StreamingContext(StreamingContextStates.Persistence));
		using MemoryStream buffer = new MemoryStream();
		using (Utf8JsonWriter writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
		{
			writer.WriteStartObject();
			writer.WriteNumber("Format", 1);
			writer.WriteStartArray("Entries");
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == SymbolTableEntry && entry.Value is IDictionary<string, object> symbols)
				{
					writer.WriteStartObject();
					writer.WriteString("Name", entry.Name);
					writer.WriteStartArray("Symbols");
					foreach (KeyValuePair<string, object> symbol in symbols)
					{
						WriteValue(writer, symbol.Key, symbol.Value);
					}
					writer.WriteEndArray();
					writer.WriteEndObject();
				}
				else
				{
					WriteValue(writer, entry.Name, entry.Value);
				}
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
		string path = Path.Combine(directory, FileName);
		string temporary = path + ".tmp";
		File.WriteAllBytes(temporary, buffer.ToArray());
		File.Move(temporary, path, overwrite: true);
	}

	internal static ExecutionContext Load(string directory)
	{
		string path = Path.Combine(directory, FileName);
		if (!File.Exists(path))
		{
			string legacyPath = Path.Combine(directory, LegacyFileName);
			if (!File.Exists(legacyPath))
			{
				throw new InvalidDataException($"Context {Path.GetFileName(directory)} cannot be loaded: {FileName} is missing.");
			}
			ExecutionContext imported = ExecutionContext.Hydrate(ReadLegacy(legacyPath));
			Save(imported, directory);
			File.Move(legacyPath, legacyPath + ".imported", overwrite: true);
			LogHelper.Instance.Log("[ExecutionContextStore] Imported context {0} ( {1} ) saved by the .NET Framework HAL.", imported.ID, imported.ProgramName);
			return imported;
		}
		using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(path));
		SerializationInfo info = new SerializationInfo(typeof(ExecutionContext), new FormatterConverter());
		foreach (JsonElement entry in document.RootElement.GetProperty("Entries").EnumerateArray())
		{
			string name = entry.GetProperty("Name").GetString();
			if (entry.TryGetProperty("Symbols", out JsonElement symbols))
			{
				Dictionary<string, object> table = new Dictionary<string, object>();
				foreach (JsonElement symbol in symbols.EnumerateArray())
				{
					ReadValue(symbol, out Type type, out object value);
					table[symbol.GetProperty("Name").GetString()] = value;
				}
				info.AddValue(name, table, table.GetType());
			}
			else
			{
				ReadValue(entry, out Type type, out object value);
				info.AddValue(name, value, type);
			}
		}
		return ExecutionContext.Hydrate(info);
	}

	internal static void SaveProgram(string directory, string programName, byte[] source)
	{
		if (source == null)
		{
			throw new InvalidOperationException($"The source of program {programName} is not known.");
		}
		File.WriteAllBytes(Path.Combine(directory, programName + SourceExtension), source);
	}

	internal static ProgramInstruction LoadProgram(string directory, string programName, out byte[] source)
	{
		string path = Path.Combine(directory, programName + SourceExtension);
		if (File.Exists(path))
		{
			source = File.ReadAllBytes(path);
		}
		else
		{
			source = CurrentProgramSource?.Invoke(programName) ?? throw new FileNotFoundException($"Program {programName} has no saved source and no current script.", path);
			LogHelper.Instance.Log("[ExecutionContextStore] Context {0}: program {1} has no saved source; using the current script.", Path.GetFileName(directory), programName);
		}
		ExecutionResult result = new Compiler(new MemoryStream(source), programName).Compile(out ProgramInstruction program);
		if (result.Errors.ContainsError())
		{
			throw new InvalidDataException($"Program {programName} no longer compiles: {result.Errors.ErrorCount} errors.");
		}
		return program;
	}

	// ctx.data as the .NET Framework HAL wrote it: an ExecutionContext record whose members are the GetObjectData entries.
	// They are strings, booleans and DateTimes, plus "Stack" (a StringBuilder) and "SymbolTable" (a Dictionary<string, object>
	// of primitives and enums).
	private static SerializationInfo ReadLegacy(string path)
	{
		ClassRecord record;
		using (FileStream stream = File.OpenRead(path))
		{
			record = NrbfDecoder.DecodeClassRecord(stream);
		}
		SerializationInfo info = new SerializationInfo(typeof(ExecutionContext), new FormatterConverter());
		foreach (string name in record.MemberNames)
		{
			object value = record.GetRawValue(name);
			if (value is ClassRecord member)
			{
				if (name == SymbolTableEntry)
				{
					Dictionary<string, object> table = ReadLegacySymbols(member);
					info.AddValue(name, table, table.GetType());
				}
				else if (member.TypeName.FullName == typeof(StringBuilder).FullName)
				{
					info.AddValue(name, member.GetString("m_StringValue") ?? string.Empty);
				}
				else
				{
					throw new InvalidDataException($"{path}: entry {name} is a {member.TypeName.FullName}, which is not expected.");
				}
			}
			else
			{
				info.AddValue(name, value, value?.GetType() ?? typeof(object));
			}
		}
		return info;
	}

	private static Dictionary<string, object> ReadLegacySymbols(ClassRecord dictionary)
	{
		Dictionary<string, object> table = new Dictionary<string, object>();
		if (dictionary.GetRawValue("KeyValuePairs") is not SZArrayRecord<SerializationRecord> pairs)
		{
			return table;
		}
		foreach (SerializationRecord item in pairs.GetArray())
		{
			if (item is not ClassRecord pair)
			{
				continue;
			}
			string key = pair.GetString("key");
			object value = pair.GetRawValue("value");
			if (value is ClassRecord boxed)
			{
				// A boxed enum: its type and the value__ field.
				Type type = Type.GetType(boxed.TypeName.FullName + ", " + boxed.TypeName.AssemblyName?.Name);
				object number = boxed.GetRawValue("value__");
				value = type != null && type.IsEnum && number != null ? Enum.ToObject(type, number) : null;
				if (value == null)
				{
					LogHelper.Instance.Log("[ExecutionContextStore] Symbol {0} has a value of type {1}, which is not read.", key, boxed.TypeName.FullName);
				}
			}
			else if (value is SerializationRecord other)
			{
				LogHelper.Instance.Log("[ExecutionContextStore] Symbol {0} has a value of type {1}, which is not read.", key, other.RecordType);
				value = null;
			}
			table[key] = value;
		}
		return table;
	}

	// A value as its type name and invariant text. What cannot be read back from text is kept as its text, the way the
	// context already keeps its data stack.
	private static void WriteValue(Utf8JsonWriter writer, string name, object value)
	{
		writer.WriteStartObject();
		writer.WriteString("Name", name);
		if (value == null)
		{
			writer.WriteNull("Type");
			writer.WriteNull("Value");
		}
		else
		{
			Type type = IsRestorable(value.GetType()) ? value.GetType() : typeof(string);
			writer.WriteString("Type", type.FullName + ", " + type.Assembly.GetName().Name);
			writer.WriteString("Value", ToText(value));
		}
		writer.WriteEndObject();
	}

	private static void ReadValue(JsonElement element, out Type type, out object value)
	{
		string typeName = element.GetProperty("Type").GetString();
		string text = element.GetProperty("Value").GetString();
		if (typeName == null)
		{
			type = typeof(object);
			value = null;
			return;
		}
		type = Type.GetType(typeName);
		if (type == null)
		{
			LogHelper.Instance.Log("[ExecutionContextStore] Type {0} of {1} is not known; the value is kept as text.", typeName, element.GetProperty("Name").GetString());
			type = typeof(string);
		}
		value = FromText(text, type);
	}

	private static bool IsRestorable(Type type)
	{
		return type == typeof(string) || type.IsEnum || type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid);
	}

	private static string ToText(object value)
	{
		return value switch
		{
			string text => text,
			DateTime time => time.ToString("O", CultureInfo.InvariantCulture),
			TimeSpan span => span.ToString("c", CultureInfo.InvariantCulture),
			StringBuilder builder => builder.ToString(),
			_ => Convert.ToString(value, CultureInfo.InvariantCulture)
		};
	}

	private static object FromText(string text, Type type)
	{
		if (text == null || type == typeof(string))
		{
			return text;
		}
		if (type == typeof(DateTime))
		{
			return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}
		if (type == typeof(TimeSpan))
		{
			return TimeSpan.ParseExact(text, "c", CultureInfo.InvariantCulture);
		}
		if (type == typeof(Guid))
		{
			return Guid.Parse(text);
		}
		if (type.IsEnum)
		{
			return Enum.Parse(type, text);
		}
		if (typeof(IConvertible).IsAssignableFrom(type))
		{
			return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
		}
		return ConversionHelper.ChangeType(text, type);
	}
}
#endif
