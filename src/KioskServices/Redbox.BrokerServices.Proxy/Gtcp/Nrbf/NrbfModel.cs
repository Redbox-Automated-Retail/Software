using System;
using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.Gtcp.Nrbf;

// MS-NRBF ([MS-NRBF] 2.1-2.7) enumerations and the object model the reader produces. Section numbers refer to the
// MS-NRBF specification; gtcp-protocol.md section 5 shows the records the broker actually exchanges.

internal enum NrbfRecordType : byte
{
	SerializedStreamHeader = 0,
	ClassWithId = 1,
	SystemClassWithMembers = 2,
	ClassWithMembers = 3,
	SystemClassWithMembersAndTypes = 4,
	ClassWithMembersAndTypes = 5,
	BinaryObjectString = 6,
	BinaryArray = 7,
	MemberPrimitiveTyped = 8,
	MemberReference = 9,
	ObjectNull = 10,
	MessageEnd = 11,
	BinaryLibrary = 12,
	ObjectNullMultiple256 = 13,
	ObjectNullMultiple = 14,
	ArraySinglePrimitive = 15,
	ArraySingleObject = 16,
	ArraySingleString = 17,
	MethodCall = 21,
	MethodReturn = 22
}

internal enum NrbfBinaryType : byte
{
	Primitive = 0,
	String = 1,
	Object = 2,
	SystemClass = 3,
	Class = 4,
	ObjectArray = 5,
	StringArray = 6,
	PrimitiveArray = 7
}

internal enum NrbfPrimitiveType : byte
{
	None = 0,
	Boolean = 1,
	Byte = 2,
	Char = 3,
	Decimal = 5,
	Double = 6,
	Int16 = 7,
	Int32 = 8,
	Int64 = 9,
	SByte = 10,
	Single = 11,
	TimeSpan = 12,
	DateTime = 13,
	UInt16 = 14,
	UInt32 = 15,
	UInt64 = 16,
	Null = 17,
	String = 18
}

/// <summary>MS-NRBF 2.2.1.1 MessageFlags of BinaryMethodCall / BinaryMethodReturn.</summary>
[Flags]
internal enum NrbfMessageFlags
{
	None = 0,
	NoArgs = 0x1,
	ArgsInline = 0x2,
	ArgsIsArray = 0x4,
	ArgsInArray = 0x8,
	NoContext = 0x10,
	ContextInline = 0x20,
	ContextInArray = 0x40,
	MethodSignatureInArray = 0x80,
	PropertiesInArray = 0x100,
	NoReturnValue = 0x200,
	ReturnValueVoid = 0x400,
	ReturnValueInline = 0x800,
	ReturnValueInArray = 0x1000,
	ExceptionInArray = 0x2000,
	GenericMethod = 0x8000
}

/// <summary>A member's declared type: MS-NRBF BinaryTypeEnum plus its AdditionalInfo.</summary>
internal readonly record struct NrbfMemberType(NrbfBinaryType BinaryType, NrbfPrimitiveType Primitive = NrbfPrimitiveType.None,
	string ClassName = null, string AssemblyName = null)
{
	public static readonly NrbfMemberType String = new NrbfMemberType(NrbfBinaryType.String);
	public static readonly NrbfMemberType Object = new NrbfMemberType(NrbfBinaryType.Object);
	public static readonly NrbfMemberType StringArray = new NrbfMemberType(NrbfBinaryType.StringArray);
	public static readonly NrbfMemberType ObjectArray = new NrbfMemberType(NrbfBinaryType.ObjectArray);

	public static NrbfMemberType Of(NrbfPrimitiveType primitive) => new NrbfMemberType(NrbfBinaryType.Primitive, primitive);

	public static NrbfMemberType SystemClass(string name) => new NrbfMemberType(NrbfBinaryType.SystemClass, ClassName: name);
}

/// <summary>A deserialized class instance: its type name, library and members in stream order.</summary>
internal sealed class NrbfObject
{
	public NrbfObject(int id, string typeName, string libraryName, IReadOnlyList<string> memberNames)
	{
		Id = id;
		TypeName = typeName;
		LibraryName = libraryName;
		MemberNames = memberNames;
		Values = new object[memberNames.Count];
	}

	public int Id { get; }

	public string TypeName { get; }

	/// <summary>Null for system (mscorlib) classes.</summary>
	public string LibraryName { get; }

	public IReadOnlyList<string> MemberNames { get; }

	internal object[] Values { get; }

	public bool TryGetMember(string name, out object value)
	{
		for (int i = 0; i < MemberNames.Count; i++)
		{
			if (MemberNames[i] == name)
			{
				value = Values[i];
				return true;
			}
		}
		value = null;
		return false;
	}

	public object this[string name] => TryGetMember(name, out object value) ? value : null;

	public override string ToString() => $"{TypeName} (id {Id})";
}

/// <summary>A deserialized single-dimension array of any element type.</summary>
internal sealed class NrbfArray
{
	public NrbfArray(int id, object[] items)
	{
		Id = id;
		Items = items;
	}

	public int Id { get; }

	public object[] Items { get; }

	public override string ToString() => $"array[{Items.Length}] (id {Id})";
}

/// <summary>A MemberReference not resolved yet; the reader replaces these once the whole stream is read.</summary>
internal sealed class NrbfReference
{
	public NrbfReference(int id)
	{
		Id = id;
	}

	public int Id { get; }
}

/// <summary>A BinaryMethodCall with its arguments and call context taken out of the call array.</summary>
internal sealed class NrbfMethodCall
{
	public string MethodName { get; init; }

	public string TypeName { get; init; }

	public NrbfMessageFlags Flags { get; init; }

	public object[] Args { get; init; } = Array.Empty<object>();
}

/// <summary>A BinaryMethodReturn with its return value or exception taken out of the call array.</summary>
internal sealed class NrbfMethodReturn
{
	public NrbfMessageFlags Flags { get; init; }

	public object ReturnValue { get; init; }

	/// <summary>The serialized exception (usually an <see cref="NrbfObject"/> of a System.Exception type), if any.</summary>
	public object Exception { get; init; }

	public bool IsException => (Flags & NrbfMessageFlags.ExceptionInArray) != 0;
}
