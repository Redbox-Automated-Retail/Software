using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Redbox.Remoting.Nrbf;

/// <summary>
/// Reads an MS-NRBF stream as written by BinaryFormatter for .NET Remoting messages (gtcp-protocol.md section 5).
/// System.Formats.Nrbf does not support the BinaryMethodCall / BinaryMethodReturn records (0x15 / 0x16), so this is
/// a small self-contained reader. It produces an untyped graph (<see cref="NrbfObject"/>, <see cref="NrbfArray"/>,
/// strings and boxed primitives); <see cref="NrbfObjectMapper"/> turns that into real .NET objects.
/// </summary>
internal sealed class NrbfReader
{
	private sealed record ClassMetadata(string Name, string LibraryName, string[] MemberNames, NrbfMemberType[] MemberTypes);

	private readonly WireReader _reader;
	private readonly Dictionary<int, object> _objects = new Dictionary<int, object>();
	private readonly Dictionary<int, ClassMetadata> _metadata = new Dictionary<int, ClassMetadata>();
	private readonly Dictionary<int, string> _libraries = new Dictionary<int, string>();
	private readonly List<NrbfObject> _allObjects = new List<NrbfObject>();
	private readonly List<NrbfArray> _allArrays = new List<NrbfArray>();

	private int _rootId;
	private NrbfMessageFlags _flags;
	private bool _isCall;
	private bool _isReturn;
	private string _methodName;
	private string _typeName;
	private object[] _inlineArgs;
	private object _inlineReturnValue;

	private NrbfReader(byte[] data)
	{
		_reader = new WireReader(data);
	}

	public static NrbfMethodCall ReadMethodCall(byte[] content)
	{
		NrbfReader reader = new NrbfReader(content);
		reader.ReadStream();
		if (!reader._isCall)
		{
			throw new InvalidDataException("The NRBF stream does not contain a BinaryMethodCall.");
		}
		return reader.BuildCall();
	}

	public static NrbfMethodReturn ReadMethodReturn(byte[] content)
	{
		NrbfReader reader = new NrbfReader(content);
		reader.ReadStream();
		if (!reader._isReturn)
		{
			throw new InvalidDataException("The NRBF stream does not contain a BinaryMethodReturn.");
		}
		return reader.BuildReturn();
	}

	/// <summary>Reads a plain serialized object graph (no method record) and returns its root object.</summary>
	public static object ReadGraph(byte[] content)
	{
		NrbfReader reader = new NrbfReader(content);
		reader.ReadStream();
		return reader.Root();
	}

	private void ReadStream()
	{
		if ((NrbfRecordType)_reader.ReadByte() != NrbfRecordType.SerializedStreamHeader)
		{
			throw new InvalidDataException("The NRBF stream does not start with a SerializedStreamHeader.");
		}
		_rootId = _reader.ReadInt32();
		_reader.ReadInt32(); // header id
		int major = _reader.ReadInt32();
		_reader.ReadInt32(); // minor
		if (major != 1)
		{
			throw new InvalidDataException($"Unsupported NRBF version {major}.");
		}
		while (true)
		{
			NrbfRecordType type = (NrbfRecordType)_reader.ReadByte();
			if (type == NrbfRecordType.MessageEnd)
			{
				break;
			}
			switch (type)
			{
				case NrbfRecordType.MethodCall:
					ReadMethodCallRecord();
					break;
				case NrbfRecordType.MethodReturn:
					ReadMethodReturnRecord();
					break;
				default:
					ReadRecordBody(type, out _);
					break;
			}
		}
		ResolveReferences();
	}

	// MS-NRBF 2.2.3.1 BinaryMethodCall: flags, MethodName, TypeName, [CallContext], [Args] (inline parts only).
	private void ReadMethodCallRecord()
	{
		_isCall = true;
		_flags = (NrbfMessageFlags)_reader.ReadInt32();
		_methodName = (string)ReadValueWithCode();
		_typeName = (string)ReadValueWithCode();
		if ((_flags & NrbfMessageFlags.ContextInline) != 0)
		{
			ReadValueWithCode();
		}
		if ((_flags & NrbfMessageFlags.ArgsInline) != 0)
		{
			_inlineArgs = ReadArrayOfValueWithCode();
		}
	}

	// MS-NRBF 2.2.3.3 BinaryMethodReturn: flags, [ReturnValue], [CallContext], [Args] (inline parts only).
	private void ReadMethodReturnRecord()
	{
		_isReturn = true;
		_flags = (NrbfMessageFlags)_reader.ReadInt32();
		if ((_flags & NrbfMessageFlags.ReturnValueInline) != 0)
		{
			_inlineReturnValue = ReadValueWithCode();
		}
		if ((_flags & NrbfMessageFlags.ContextInline) != 0)
		{
			ReadValueWithCode();
		}
		if ((_flags & NrbfMessageFlags.ArgsInline) != 0)
		{
			_inlineArgs = ReadArrayOfValueWithCode();
		}
	}

	// The call array (MS-NRBF 2.2.3.2) holds, in this order, whatever the flags put "InArray".
	private NrbfMethodCall BuildCall()
	{
		object[] args = _inlineArgs ?? Array.Empty<object>();
		object[] callArray = (Root() as NrbfArray)?.Items;
		if ((_flags & NrbfMessageFlags.ArgsIsArray) != 0)
		{
			args = callArray ?? Array.Empty<object>();
		}
		else if ((_flags & NrbfMessageFlags.ArgsInArray) != 0)
		{
			args = ((callArray?[0] as NrbfArray) ?? throw new InvalidDataException("Missing argument array.")).Items;
		}
		return new NrbfMethodCall { MethodName = _methodName, TypeName = _typeName, Flags = _flags, Args = args };
	}

	private NrbfMethodReturn BuildReturn()
	{
		object returnValue = _inlineReturnValue;
		object exception = null;
		object[] callArray = (Root() as NrbfArray)?.Items;
		if (callArray != null && (_flags & NrbfMessageFlags.ArgsIsArray) == 0)
		{
			int index = 0;
			if ((_flags & NrbfMessageFlags.ReturnValueInArray) != 0)
			{
				returnValue = callArray[index++];
			}
			if ((_flags & NrbfMessageFlags.ArgsInArray) != 0)
			{
				index++;
			}
			if ((_flags & NrbfMessageFlags.ExceptionInArray) != 0)
			{
				exception = callArray[index];
			}
		}
		return new NrbfMethodReturn { Flags = _flags, ReturnValue = returnValue, Exception = exception };
	}

	private object Root() => _rootId != 0 && _objects.TryGetValue(_rootId, out object root) ? root : null;

	/// <summary>
	/// Reads the record that stands for one value (a member, an array element or a top-level object). BinaryLibrary
	/// records may precede it. <paramref name="nullCount"/> is set for ObjectNullMultiple records.
	/// </summary>
	private object ReadValueRecord(out int nullCount)
	{
		NrbfRecordType type = (NrbfRecordType)_reader.ReadByte();
		return ReadRecordBody(type, out nullCount);
	}

	private object ReadRecordBody(NrbfRecordType type, out int nullCount)
	{
		nullCount = 0;
		switch (type)
		{
			case NrbfRecordType.BinaryLibrary:
				_libraries[_reader.ReadInt32()] = _reader.ReadString();
				return ReadValueRecord(out nullCount);
			case NrbfRecordType.ClassWithId:
			{
				int id = _reader.ReadInt32();
				ClassMetadata metadata = _metadata[_reader.ReadInt32()];
				_metadata[id] = metadata;
				return ReadClassMembers(id, metadata);
			}
			case NrbfRecordType.SystemClassWithMembers:
			case NrbfRecordType.ClassWithMembers:
			case NrbfRecordType.SystemClassWithMembersAndTypes:
			case NrbfRecordType.ClassWithMembersAndTypes:
				return ReadClass(type);
			case NrbfRecordType.BinaryObjectString:
			{
				int id = _reader.ReadInt32();
				string value = _reader.ReadString();
				_objects[id] = value;
				return value;
			}
			case NrbfRecordType.MemberPrimitiveTyped:
				return ReadPrimitive((NrbfPrimitiveType)_reader.ReadByte());
			case NrbfRecordType.MemberReference:
				return new NrbfReference(_reader.ReadInt32());
			case NrbfRecordType.ObjectNull:
				nullCount = 1;
				return null;
			case NrbfRecordType.ObjectNullMultiple256:
				nullCount = _reader.ReadByte();
				return null;
			case NrbfRecordType.ObjectNullMultiple:
				nullCount = _reader.ReadInt32();
				return null;
			case NrbfRecordType.ArraySinglePrimitive:
			{
				int id = _reader.ReadInt32();
				int length = _reader.ReadInt32();
				NrbfPrimitiveType primitive = (NrbfPrimitiveType)_reader.ReadByte();
				object[] items = new object[length];
				for (int i = 0; i < length; i++)
				{
					items[i] = ReadPrimitive(primitive);
				}
				return Register(new NrbfArray(id, items));
			}
			case NrbfRecordType.ArraySingleObject:
			case NrbfRecordType.ArraySingleString:
			{
				int id = _reader.ReadInt32();
				int length = _reader.ReadInt32();
				return Register(new NrbfArray(id, ReadElements(length)));
			}
			case NrbfRecordType.BinaryArray:
				return ReadBinaryArray();
			default:
				throw new InvalidDataException($"Unsupported NRBF record type {(byte)type} at offset {_reader.Position - 1}.");
		}
	}

	private NrbfObject ReadClass(NrbfRecordType type)
	{
		int id = _reader.ReadInt32();
		string name = _reader.ReadString();
		int count = _reader.ReadInt32();
		string[] memberNames = new string[count];
		for (int i = 0; i < count; i++)
		{
			memberNames[i] = _reader.ReadString();
		}
		NrbfMemberType[] memberTypes = null;
		if (type == NrbfRecordType.SystemClassWithMembersAndTypes || type == NrbfRecordType.ClassWithMembersAndTypes)
		{
			memberTypes = ReadMemberTypeInfo(count);
		}
		string library = null;
		if (type == NrbfRecordType.ClassWithMembers || type == NrbfRecordType.ClassWithMembersAndTypes)
		{
			int libraryId = _reader.ReadInt32();
			library = _libraries.TryGetValue(libraryId, out string libraryName) ? libraryName : null;
		}
		ClassMetadata metadata = new ClassMetadata(name, library, memberNames, memberTypes);
		_metadata[id] = metadata;
		return ReadClassMembers(id, metadata);
	}

	private NrbfObject ReadClassMembers(int id, ClassMetadata metadata)
	{
		NrbfObject record = new NrbfObject(id, metadata.Name, metadata.LibraryName, metadata.MemberNames);
		Register(record);
		for (int i = 0; i < metadata.MemberNames.Length; i++)
		{
			NrbfMemberType? memberType = metadata.MemberTypes?[i];
			record.Values[i] = memberType is { BinaryType: NrbfBinaryType.Primitive }
				? ReadPrimitive(memberType.Value.Primitive)
				: ReadValueRecord(out _);
		}
		return record;
	}

	// MS-NRBF 2.3.1.2 MemberTypeInfo: all BinaryTypeEnums first, then the AdditionalInfo of those that have one.
	private NrbfMemberType[] ReadMemberTypeInfo(int count)
	{
		NrbfBinaryType[] binaryTypes = new NrbfBinaryType[count];
		for (int i = 0; i < count; i++)
		{
			binaryTypes[i] = (NrbfBinaryType)_reader.ReadByte();
		}
		NrbfMemberType[] types = new NrbfMemberType[count];
		for (int i = 0; i < count; i++)
		{
			types[i] = ReadAdditionalInfo(binaryTypes[i]);
		}
		return types;
	}

	private NrbfMemberType ReadAdditionalInfo(NrbfBinaryType binaryType)
	{
		switch (binaryType)
		{
			case NrbfBinaryType.Primitive:
			case NrbfBinaryType.PrimitiveArray:
				return new NrbfMemberType(binaryType, (NrbfPrimitiveType)_reader.ReadByte());
			case NrbfBinaryType.SystemClass:
				return new NrbfMemberType(binaryType, ClassName: _reader.ReadString());
			case NrbfBinaryType.Class:
			{
				string name = _reader.ReadString();
				int libraryId = _reader.ReadInt32();
				return new NrbfMemberType(binaryType, ClassName: name, AssemblyName: _libraries.GetValueOrDefault(libraryId));
			}
			default:
				return new NrbfMemberType(binaryType);
		}
	}

	// MS-NRBF 2.4.3.1 BinaryArray. Only single-dimension arrays occur in remoting messages; jagged/offset arrays are
	// read as flat arrays, which is enough for the broker's types.
	private NrbfArray ReadBinaryArray()
	{
		int id = _reader.ReadInt32();
		byte arrayType = _reader.ReadByte();
		int rank = _reader.ReadInt32();
		int total = 1;
		for (int i = 0; i < rank; i++)
		{
			total *= _reader.ReadInt32();
		}
		if (arrayType >= 3)
		{
			for (int i = 0; i < rank; i++)
			{
				_reader.ReadInt32(); // lower bounds
			}
		}
		NrbfMemberType elementType = ReadAdditionalInfo((NrbfBinaryType)_reader.ReadByte());
		object[] items;
		if (elementType.BinaryType == NrbfBinaryType.Primitive)
		{
			items = new object[total];
			for (int i = 0; i < total; i++)
			{
				items[i] = ReadPrimitive(elementType.Primitive);
			}
		}
		else
		{
			items = ReadElements(total);
		}
		return Register(new NrbfArray(id, items));
	}

	private object[] ReadElements(int length)
	{
		object[] items = new object[length];
		int index = 0;
		while (index < length)
		{
			object value = ReadValueRecord(out int nullCount);
			if (nullCount > 0)
			{
				index += nullCount; // the slots are already null
			}
			else
			{
				items[index++] = value;
			}
		}
		return items;
	}

	private object ReadValueWithCode()
	{
		return ReadPrimitive((NrbfPrimitiveType)_reader.ReadByte());
	}

	private object[] ReadArrayOfValueWithCode()
	{
		int length = _reader.ReadInt32();
		object[] values = new object[length];
		for (int i = 0; i < length; i++)
		{
			values[i] = ReadValueWithCode();
		}
		return values;
	}

	private object ReadPrimitive(NrbfPrimitiveType type)
	{
		switch (type)
		{
			case NrbfPrimitiveType.Boolean: return _reader.ReadBoolean();
			case NrbfPrimitiveType.Byte: return _reader.ReadByte();
			case NrbfPrimitiveType.Char: return _reader.ReadUtf8Char();
			// Decimal is a LengthPrefixedString in invariant culture, e.g. "1.99" (gtcp-protocol.md 5.2).
			case NrbfPrimitiveType.Decimal: return decimal.Parse(_reader.ReadString(), NumberStyles.Number, CultureInfo.InvariantCulture);
			case NrbfPrimitiveType.Double: return _reader.ReadDouble();
			case NrbfPrimitiveType.Int16: return _reader.ReadInt16();
			case NrbfPrimitiveType.Int32: return _reader.ReadInt32();
			case NrbfPrimitiveType.Int64: return _reader.ReadInt64();
			case NrbfPrimitiveType.SByte: return (sbyte)_reader.ReadByte();
			case NrbfPrimitiveType.Single: return _reader.ReadSingle();
			case NrbfPrimitiveType.TimeSpan: return new TimeSpan(_reader.ReadInt64());
			case NrbfPrimitiveType.DateTime: return NrbfDateTime.FromWire(_reader.ReadUInt64());
			case NrbfPrimitiveType.UInt16: return _reader.ReadUInt16();
			case NrbfPrimitiveType.UInt32: return _reader.ReadUInt32();
			case NrbfPrimitiveType.UInt64: return _reader.ReadUInt64();
			case NrbfPrimitiveType.Null: return null;
			case NrbfPrimitiveType.String: return _reader.ReadString();
			default: throw new InvalidDataException($"Unsupported NRBF primitive type {(byte)type}.");
		}
	}

	private T Register<T>(T value) where T : class
	{
		switch (value)
		{
			case NrbfObject objectRecord:
				_objects[objectRecord.Id] = objectRecord;
				_allObjects.Add(objectRecord);
				break;
			case NrbfArray arrayRecord:
				_objects[arrayRecord.Id] = arrayRecord;
				_allArrays.Add(arrayRecord);
				break;
		}
		return value;
	}

	private void ResolveReferences()
	{
		foreach (NrbfObject objectRecord in _allObjects)
		{
			Resolve(objectRecord.Values);
		}
		foreach (NrbfArray arrayRecord in _allArrays)
		{
			Resolve(arrayRecord.Items);
		}
	}

	private void Resolve(object[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i] is NrbfReference reference)
			{
				values[i] = _objects.TryGetValue(reference.Id, out object target)
					? target
					: throw new InvalidDataException($"NRBF reference to unknown object {reference.Id}.");
			}
		}
	}
}

/// <summary>
/// DateTime on the wire: Int64 with the DateTimeKind in bits 62-63 (0 Unspecified, 1 Utc, 2 Local, 3 Local in the
/// ambiguous daylight hour) and the ticks in bits 0-61 (gtcp-protocol.md 5.2). BinaryFormatter writes the raw value, so
/// a Local time keeps its local ticks.
/// </summary>
internal static class NrbfDateTime
{
	private const ulong TicksMask = 0x3FFFFFFFFFFFFFFFUL;

	public static DateTime FromWire(ulong value)
	{
		long ticks = (long)(value & TicksMask);
		DateTimeKind kind = (value >> 62) switch
		{
			1 => DateTimeKind.Utc,
			0 => DateTimeKind.Unspecified,
			_ => DateTimeKind.Local
		};
		return new DateTime(ticks, kind);
	}

	public static ulong ToWire(DateTime value)
	{
		ulong kind = value.Kind switch
		{
			DateTimeKind.Utc => 1UL,
			DateTimeKind.Local => 2UL,
			_ => 0UL
		};
		return (ulong)value.Ticks | (kind << 62);
	}
}
