using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
// SYSLIB0050: Type.IsSerializable / FieldInfo.IsNotSerialized are "formatter-based serialization" APIs; this writer
// reproduces BinaryFormatter's output on purpose (the broker is a BinaryFormatter), so it needs the same metadata.
#pragma warning disable SYSLIB0050

namespace Redbox.Remoting.Nrbf;

/// <summary>
/// A class instance described member by member, for objects that have no .NET type here (the remoting ObjRef graph,
/// a serialized System.Exception).
/// </summary>
internal sealed class NrbfSyntheticObject
{
	public NrbfSyntheticObject(string typeName, string? assemblyName, params (string Name, NrbfMemberType Type, object? Value)[] members)
	{
		TypeName = typeName;
		AssemblyName = assemblyName;
		Members = members;
	}

	public string TypeName { get; }

	/// <summary>Null for mscorlib types.</summary>
	public string? AssemblyName { get; }

	public (string Name, NrbfMemberType Type, object? Value)[] Members { get; }
}

/// <summary>
/// Writes MS-NRBF the way .NET Framework's BinaryFormatter does, so the broker (a .NET Framework / Mono BinaryFormatter)
/// reads it back: the top object first, then every referenced object breadth-first in the order it was first met, one
/// id counter shared by objects and BinaryLibrary records, metadata reused with ClassWithId, strings inline, runs of
/// nulls in arrays collapsed, nested value types (enums) inline with a negative id. With these rules the replies are
/// byte-identical to the real client's (checked against the captures in tests\BrokerGtcp.Check).
/// </summary>
internal sealed class NrbfWriter
{
	private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldCache = new ConcurrentDictionary<Type, FieldInfo[]>();
	private static readonly ConcurrentDictionary<Type, string[]> FieldNameCache = new ConcurrentDictionary<Type, string[]>();

	private readonly MemoryStream _out = new MemoryStream();
	private readonly Dictionary<object, int> _ids = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
	private readonly Dictionary<string, int> _libraries = new Dictionary<string, int>(StringComparer.Ordinal);
	private readonly Dictionary<string, (int Id, NrbfMemberType[] Types)> _classes = new Dictionary<string, (int, NrbfMemberType[])>(StringComparer.Ordinal);
	private readonly Queue<(object Value, int Id)> _pending = new Queue<(object, int)>();
	private int _lastId;

	private NrbfWriter()
	{
	}

	// ---------------------------------------------------------------- remoting messages (gtcp-protocol.md 5.1 / 5.2)

	/// <summary>
	/// A BinaryMethodCall. Primitive/string arguments go inline (ArgsInline); otherwise they go in an argument array
	/// wrapped by the call array (ArgsInArray: callArray = { args }), which is what the broker expects (5.1).
	/// </summary>
	public static byte[] MethodCall(string methodName, string typeName, object?[] args)
	{
		NrbfWriter writer = new NrbfWriter();
		bool inline = args.All(IsInlineValue);
		NrbfMessageFlags flags = NrbfMessageFlags.NoContext
			| (args.Length == 0 ? NrbfMessageFlags.NoArgs : inline ? NrbfMessageFlags.ArgsInline : NrbfMessageFlags.ArgsInArray);
		writer.WriteHeader(inline ? 0 : 1, inline ? 0 : -1);
		writer._out.WriteByte((byte)NrbfRecordType.MethodCall);
		WireBinary.WriteInt32(writer._out, (int)flags);
		writer.WriteValueWithCode(methodName);
		writer.WriteValueWithCode(typeName);
		if (inline)
		{
			if (args.Length > 0)
			{
				writer.WriteArrayOfValueWithCode(args);
			}
		}
		else
		{
			writer.WriteTopObject(new object[] { args });
		}
		writer._out.WriteByte((byte)NrbfRecordType.MessageEnd);
		return writer._out.ToArray();
	}

	/// <summary>
	/// A BinaryMethodReturn for a successful call. Void → ReturnValueVoid; null → NoReturnValue; a primitive or string →
	/// ReturnValueInline; an object graph → ReturnValueInArray (callArray = { returnValue }), as the real client does.
	/// </summary>
	public static byte[] MethodReturn(object? returnValue, bool isVoid)
	{
		NrbfWriter writer = new NrbfWriter();
		NrbfMessageFlags flags = NrbfMessageFlags.NoArgs | NrbfMessageFlags.NoContext;
		bool inArray = false;
		if (isVoid)
		{
			flags |= NrbfMessageFlags.ReturnValueVoid;
		}
		else if (returnValue == null)
		{
			flags |= NrbfMessageFlags.NoReturnValue;
		}
		else if (IsInlineValue(returnValue))
		{
			flags |= NrbfMessageFlags.ReturnValueInline;
		}
		else
		{
			flags |= NrbfMessageFlags.ReturnValueInArray;
			inArray = true;
		}
		writer.WriteHeader(inArray ? 1 : 0, inArray ? -1 : 0);
		writer._out.WriteByte((byte)NrbfRecordType.MethodReturn);
		WireBinary.WriteInt32(writer._out, (int)flags);
		if ((flags & NrbfMessageFlags.ReturnValueInline) != 0)
		{
			writer.WriteValueWithCode(returnValue);
		}
		if (inArray)
		{
			writer.WriteTopObject(new object?[] { returnValue });
		}
		writer._out.WriteByte((byte)NrbfRecordType.MessageEnd);
		return writer._out.ToArray();
	}

	/// <summary>A BinaryMethodReturn carrying an exception (ExceptionInArray), as a .NET Framework System.Exception graph.</summary>
	public static byte[] ExceptionReturn(Exception exception)
	{
		NrbfWriter writer = new NrbfWriter();
		NrbfMessageFlags flags = NrbfMessageFlags.NoArgs | NrbfMessageFlags.NoContext | NrbfMessageFlags.NoReturnValue | NrbfMessageFlags.ExceptionInArray;
		writer.WriteHeader(1, -1);
		writer._out.WriteByte((byte)NrbfRecordType.MethodReturn);
		WireBinary.WriteInt32(writer._out, (int)flags);
		writer.WriteTopObject(new object[] { DescribeException(exception) });
		writer._out.WriteByte((byte)NrbfRecordType.MessageEnd);
		return writer._out.ToArray();
	}

	// The member layout .NET Framework's Exception.GetObjectData writes.
	private static NrbfSyntheticObject DescribeException(Exception exception) => new NrbfSyntheticObject("System.Exception", null,
		("ClassName", NrbfMemberType.String, "System.Exception"),
		("Message", NrbfMemberType.String, exception.GetType().FullName + ": " + exception.Message),
		("Data", NrbfMemberType.SystemClass("System.Collections.IDictionary"), null),
		("InnerException", NrbfMemberType.SystemClass("System.Exception"), null),
		("HelpURL", NrbfMemberType.String, null),
		("StackTraceString", NrbfMemberType.String, exception.StackTrace),
		("RemoteStackTraceString", NrbfMemberType.String, null),
		("RemoteStackIndex", NrbfMemberType.Of(NrbfPrimitiveType.Int32), 0),
		("ExceptionMethod", NrbfMemberType.String, null),
		("HResult", NrbfMemberType.Of(NrbfPrimitiveType.Int32), exception.HResult),
		("Source", NrbfMemberType.String, exception.Source),
		("WatsonBuckets", new NrbfMemberType(NrbfBinaryType.PrimitiveArray, NrbfPrimitiveType.Byte), null));

	// ---------------------------------------------------------------- graph writing

	private void WriteHeader(int rootId, int headerId)
	{
		_out.WriteByte((byte)NrbfRecordType.SerializedStreamHeader);
		WireBinary.WriteInt32(_out, rootId);
		WireBinary.WriteInt32(_out, headerId);
		WireBinary.WriteInt32(_out, 1);
		WireBinary.WriteInt32(_out, 0);
	}

	private void WriteTopObject(object top)
	{
		int id = AssignId(top);
		WriteObject(top, id);
		while (_pending.Count > 0)
		{
			(object value, int pendingId) = _pending.Dequeue();
			WriteObject(value, pendingId);
		}
	}

	private int AssignId(object value)
	{
		int id = ++_lastId;
		_ids[value] = id;
		return id;
	}

	private void WriteObject(object value, int id)
	{
		switch (value)
		{
			case Array array:
				WriteArray(array, id);
				break;
			case NrbfSyntheticObject synthetic:
				WriteClass(id, synthetic.TypeName, synthetic.AssemblyName,
					synthetic.Members.Select(m => m.Name).ToArray(),
					synthetic.Members.Select(m => m.Type).ToArray(),
					synthetic.Members.Select(m => m.Value).ToArray());
				break;
			default:
				WriteReflectedObject(value, id);
				break;
		}
	}

	private void WriteReflectedObject(object value, int id)
	{
		Type type = value.GetType();
		if (type.IsEnum)
		{
			Type underlying = Enum.GetUnderlyingType(type);
			WriteClass(id, FrameworkTypeNames.TypeName(type), LibraryNameOf(type),
				new[] { "value__" },
				new[] { NrbfMemberType.Of(FrameworkTypeNames.PrimitiveOf(underlying)) },
				new[] { Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture) });
			return;
		}
		if (!type.IsSerializable)
		{
			throw new InvalidOperationException($"{type} is not serializable.");
		}
		FieldInfo[] fields = SerializableFields(type);
		WriteClass(id, FrameworkTypeNames.TypeName(type), LibraryNameOf(type),
			FieldNameCache.GetOrAdd(type, t => SerializableFieldNames(t, SerializableFields(t))),
			fields.Select(f => MemberTypeOf(f.FieldType)).ToArray(),
			fields.Select(f => f.GetValue(value)).ToArray());
	}

	/// <summary>
	/// A class record: ClassWithId when this type was written before, otherwise (System)ClassWithMembersAndTypes,
	/// preceded by the BinaryLibrary records it needs (its own assembly first, then member types' assemblies).
	/// </summary>
	private void WriteClass(int id, string typeName, string? assemblyName, string[] memberNames, NrbfMemberType[] memberTypes, object?[] values)
	{
		string key = typeName + "|" + assemblyName;
		if (_classes.TryGetValue(key, out var known))
		{
			_out.WriteByte((byte)NrbfRecordType.ClassWithId);
			WireBinary.WriteInt32(_out, id);
			WireBinary.WriteInt32(_out, known.Id);
			memberTypes = known.Types;
		}
		else
		{
			int libraryId = assemblyName == null ? 0 : EnsureLibrary(assemblyName);
			foreach (NrbfMemberType memberType in memberTypes)
			{
				if (memberType.BinaryType == NrbfBinaryType.Class)
				{
					EnsureLibrary(memberType.AssemblyName!);
				}
			}
			_out.WriteByte((byte)(assemblyName == null ? NrbfRecordType.SystemClassWithMembersAndTypes : NrbfRecordType.ClassWithMembersAndTypes));
			WireBinary.WriteInt32(_out, id);
			WireBinary.WriteString(_out, typeName);
			WireBinary.WriteInt32(_out, memberNames.Length);
			foreach (string name in memberNames)
			{
				WireBinary.WriteString(_out, name);
			}
			WriteMemberTypeInfo(memberTypes);
			if (assemblyName != null)
			{
				WireBinary.WriteInt32(_out, libraryId);
			}
			_classes[key] = (id, memberTypes);
		}
		for (int i = 0; i < values.Length; i++)
		{
			WriteMemberValue(memberTypes[i], values[i]);
		}
	}

	private void WriteMemberTypeInfo(NrbfMemberType[] memberTypes)
	{
		foreach (NrbfMemberType memberType in memberTypes)
		{
			_out.WriteByte((byte)memberType.BinaryType);
		}
		foreach (NrbfMemberType memberType in memberTypes)
		{
			WriteAdditionalInfo(memberType);
		}
	}

	private void WriteAdditionalInfo(NrbfMemberType memberType)
	{
		switch (memberType.BinaryType)
		{
			case NrbfBinaryType.Primitive:
			case NrbfBinaryType.PrimitiveArray:
				_out.WriteByte((byte)memberType.Primitive);
				break;
			case NrbfBinaryType.SystemClass:
				WireBinary.WriteString(_out, memberType.ClassName!);
				break;
			case NrbfBinaryType.Class:
				WireBinary.WriteString(_out, memberType.ClassName!);
				WireBinary.WriteInt32(_out, _libraries[memberType.AssemblyName!]);
				break;
		}
	}

	private void WriteMemberValue(NrbfMemberType memberType, object? value)
	{
		if (memberType.BinaryType == NrbfBinaryType.Primitive)
		{
			WritePrimitive(memberType.Primitive, value!);
			return;
		}
		if (value != null && value.GetType().IsValueType && memberType.BinaryType != NrbfBinaryType.Object)
		{
			// A nested value type (an enum field): written in place with a negative id that does not use up a number.
			WriteObject(value, -(_lastId + 1));
			return;
		}
		WriteReferenceOrInline(value);
	}

	/// <summary>A value in an Object/class/array slot: null, an inline string or primitive, or a reference.</summary>
	private void WriteReferenceOrInline(object? value)
	{
		switch (value)
		{
			case null:
				_out.WriteByte((byte)NrbfRecordType.ObjectNull);
				return;
			case string text:
				WriteString(text);
				return;
		}
		NrbfPrimitiveType primitive = FrameworkTypeNames.PrimitiveOf(value.GetType());
		if (primitive != NrbfPrimitiveType.None)
		{
			_out.WriteByte((byte)NrbfRecordType.MemberPrimitiveTyped);
			_out.WriteByte((byte)primitive);
			WritePrimitive(primitive, value);
			return;
		}
		if (!_ids.TryGetValue(value, out int id))
		{
			id = AssignId(value);
			_pending.Enqueue((value, id));
		}
		_out.WriteByte((byte)NrbfRecordType.MemberReference);
		WireBinary.WriteInt32(_out, id);
	}

	private void WriteString(string text)
	{
		if (_ids.TryGetValue(text, out int id))
		{
			_out.WriteByte((byte)NrbfRecordType.MemberReference);
			WireBinary.WriteInt32(_out, id);
			return;
		}
		_out.WriteByte((byte)NrbfRecordType.BinaryObjectString);
		WireBinary.WriteInt32(_out, AssignId(text));
		WireBinary.WriteString(_out, text);
	}

	private void WriteArray(Array array, int id)
	{
		Type elementType = array.GetType().GetElementType()!;
		if (array.Rank != 1)
		{
			throw new NotSupportedException("Only single-dimension arrays are supported.");
		}
		NrbfPrimitiveType primitive = FrameworkTypeNames.PrimitiveOf(elementType);
		if (primitive != NrbfPrimitiveType.None)
		{
			_out.WriteByte((byte)NrbfRecordType.ArraySinglePrimitive);
			WireBinary.WriteInt32(_out, id);
			WireBinary.WriteInt32(_out, array.Length);
			_out.WriteByte((byte)primitive);
			foreach (object item in array)
			{
				WritePrimitive(primitive, item);
			}
			return;
		}
		if (elementType == typeof(object) || elementType == typeof(string))
		{
			_out.WriteByte((byte)(elementType == typeof(object) ? NrbfRecordType.ArraySingleObject : NrbfRecordType.ArraySingleString));
			WireBinary.WriteInt32(_out, id);
			WireBinary.WriteInt32(_out, array.Length);
		}
		else
		{
			NrbfMemberType itemType = MemberTypeOf(elementType);
			if (itemType.BinaryType == NrbfBinaryType.Class)
			{
				EnsureLibrary(itemType.AssemblyName!);
			}
			_out.WriteByte((byte)NrbfRecordType.BinaryArray);
			WireBinary.WriteInt32(_out, id);
			_out.WriteByte(0); // BinaryArrayType.Single
			WireBinary.WriteInt32(_out, 1);
			WireBinary.WriteInt32(_out, array.Length);
			_out.WriteByte((byte)itemType.BinaryType);
			WriteAdditionalInfo(itemType);
		}
		WriteElements(array, elementType);
	}

	private void WriteElements(Array array, Type elementType)
	{
		int nulls = 0;
		foreach (object item in array)
		{
			if (item == null)
			{
				nulls++;
				continue;
			}
			FlushNulls(ref nulls);
			if (item.GetType().IsValueType && elementType.IsValueType)
			{
				WriteObject(item, -(_lastId + 1));
			}
			else
			{
				WriteReferenceOrInline(item);
			}
		}
		FlushNulls(ref nulls);
	}

	private void FlushNulls(ref int nulls)
	{
		if (nulls == 1)
		{
			_out.WriteByte((byte)NrbfRecordType.ObjectNull);
		}
		else if (nulls > 1 && nulls < 256)
		{
			_out.WriteByte((byte)NrbfRecordType.ObjectNullMultiple256);
			_out.WriteByte((byte)nulls);
		}
		else if (nulls >= 256)
		{
			_out.WriteByte((byte)NrbfRecordType.ObjectNullMultiple);
			WireBinary.WriteInt32(_out, nulls);
		}
		nulls = 0;
	}

	private int EnsureLibrary(string assemblyName)
	{
		if (!_libraries.TryGetValue(assemblyName, out int id))
		{
			id = ++_lastId;
			_libraries[assemblyName] = id;
			_out.WriteByte((byte)NrbfRecordType.BinaryLibrary);
			WireBinary.WriteInt32(_out, id);
			WireBinary.WriteString(_out, assemblyName);
		}
		return id;
	}

	// ---------------------------------------------------------------- values

	private static bool IsInlineValue(object? value) =>
		value == null || value is string || FrameworkTypeNames.PrimitiveOf(value.GetType()) != NrbfPrimitiveType.None;

	private void WriteValueWithCode(object? value)
	{
		if (value == null)
		{
			_out.WriteByte((byte)NrbfPrimitiveType.Null);
			return;
		}
		if (value is string text)
		{
			_out.WriteByte((byte)NrbfPrimitiveType.String);
			WireBinary.WriteString(_out, text);
			return;
		}
		NrbfPrimitiveType primitive = FrameworkTypeNames.PrimitiveOf(value.GetType());
		_out.WriteByte((byte)primitive);
		WritePrimitive(primitive, value);
	}

	private void WriteArrayOfValueWithCode(object?[] values)
	{
		WireBinary.WriteInt32(_out, values.Length);
		foreach (object? value in values)
		{
			WriteValueWithCode(value);
		}
	}

	private void WritePrimitive(NrbfPrimitiveType type, object value)
	{
		switch (type)
		{
			case NrbfPrimitiveType.Boolean: _out.WriteByte((bool)value ? (byte)1 : (byte)0); break;
			case NrbfPrimitiveType.Byte: _out.WriteByte((byte)value); break;
			case NrbfPrimitiveType.SByte: _out.WriteByte(unchecked((byte)(sbyte)value)); break;
			case NrbfPrimitiveType.Char: _out.Write(System.Text.Encoding.UTF8.GetBytes(new[] { (char)value })); break;
			case NrbfPrimitiveType.Decimal: WireBinary.WriteString(_out, ((decimal)value).ToString(CultureInfo.InvariantCulture)); break;
			case NrbfPrimitiveType.Double: WireBinary.WriteInt64(_out, BitConverter.DoubleToInt64Bits((double)value)); break;
			case NrbfPrimitiveType.Single: WireBinary.WriteInt32(_out, BitConverter.SingleToInt32Bits((float)value)); break;
			case NrbfPrimitiveType.Int16: WireBinary.WriteInt16(_out, (short)value); break;
			case NrbfPrimitiveType.UInt16: WireBinary.WriteInt16(_out, unchecked((short)(ushort)value)); break;
			case NrbfPrimitiveType.Int32: WireBinary.WriteInt32(_out, (int)value); break;
			case NrbfPrimitiveType.UInt32: WireBinary.WriteInt32(_out, unchecked((int)(uint)value)); break;
			case NrbfPrimitiveType.Int64: WireBinary.WriteInt64(_out, (long)value); break;
			case NrbfPrimitiveType.UInt64: WireBinary.WriteInt64(_out, unchecked((long)(ulong)value)); break;
			case NrbfPrimitiveType.TimeSpan: WireBinary.WriteInt64(_out, ((TimeSpan)value).Ticks); break;
			case NrbfPrimitiveType.DateTime: WireBinary.WriteInt64(_out, unchecked((long)NrbfDateTime.ToWire((DateTime)value))); break;
			default: throw new NotSupportedException($"Cannot write NRBF primitive {type}.");
		}
	}

	// ---------------------------------------------------------------- reflection

	private static string? LibraryNameOf(Type type) => FrameworkTypeNames.IsSystemType(type) ? null : FrameworkTypeNames.AssemblyName(type);

	/// <summary>The MemberTypeInfo entry BinaryFormatter writes for a field of this declared type.</summary>
	internal static NrbfMemberType MemberTypeOf(Type type)
	{
		NrbfPrimitiveType primitive = FrameworkTypeNames.PrimitiveOf(type);
		if (primitive != NrbfPrimitiveType.None)
		{
			return NrbfMemberType.Of(primitive);
		}
		if (type == typeof(string))
		{
			return NrbfMemberType.String;
		}
		if (type == typeof(object))
		{
			return NrbfMemberType.Object;
		}
		if (type.IsArray && type.GetArrayRank() == 1)
		{
			Type element = type.GetElementType()!;
			if (element == typeof(string))
			{
				return NrbfMemberType.StringArray;
			}
			if (element == typeof(object))
			{
				return NrbfMemberType.ObjectArray;
			}
			NrbfPrimitiveType elementPrimitive = FrameworkTypeNames.PrimitiveOf(element);
			if (elementPrimitive != NrbfPrimitiveType.None)
			{
				return new NrbfMemberType(NrbfBinaryType.PrimitiveArray, elementPrimitive);
			}
		}
		return FrameworkTypeNames.IsSystemType(type)
			? NrbfMemberType.SystemClass(FrameworkTypeNames.TypeName(type))
			: new NrbfMemberType(NrbfBinaryType.Class, ClassName: FrameworkTypeNames.TypeName(type), AssemblyName: FrameworkTypeNames.AssemblyName(type));
	}

	/// <summary>
	/// The fields BinaryFormatter serializes (FormatterServices.GetSerializableMembers): instance fields of the type,
	/// then of each serializable base type; [NonSerialized] fields are skipped.
	/// </summary>
	internal static FieldInfo[] SerializableFields(Type type) => FieldCache.GetOrAdd(type, t =>
	{
		List<FieldInfo> fields = new List<FieldInfo>();
		for (Type? current = t; current != null && current != typeof(object); current = current.BaseType)
		{
			fields.AddRange(current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
				.Where(f => !f.IsNotSerialized));
		}
		return fields.ToArray();
	});

	// Inherited fields are named "BaseType+field".
	private static string[] SerializableFieldNames(Type type, FieldInfo[] fields) =>
		fields.Select(f => f.DeclaringType == type ? f.Name : f.DeclaringType!.Name + "+" + f.Name).ToArray();
}
