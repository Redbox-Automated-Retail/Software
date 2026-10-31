using System;
using System.Linq;
using System.Reflection;

namespace Redbox.BrokerServices.Proxy.Gtcp.Nrbf;

/// <summary>
/// Type and assembly names as .NET Framework's BinaryFormatter writes them. The broker deserializes by name, so types
/// from System.Private.CoreLib must be written as mscorlib types ("SystemClass" records, generic arguments qualified
/// with mscorlib), and our own assemblies under their real full names (KioskShared is
/// "Redbox.Services.KioskBrokerServices.KioskShared, Version=0.0.0.56, Culture=neutral, PublicKeyToken=null", as in
/// the captures).
/// </summary>
internal static class FrameworkTypeNames
{
	public const string Mscorlib = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	private static readonly Assembly CoreLibrary = typeof(object).Assembly;

	/// <summary>True when the Framework type lives in mscorlib (written without a BinaryLibrary).</summary>
	public static bool IsSystemType(Type type)
	{
		while (type.IsArray)
		{
			type = type.GetElementType();
		}
		if (type.IsGenericType)
		{
			type = type.GetGenericTypeDefinition();
		}
		return type.Assembly == CoreLibrary;
	}

	/// <summary>Full name with generic arguments as [[name, assembly]] (System.Collections.Generic.List`1[[...]]).</summary>
	public static string TypeName(Type type)
	{
		if (type.IsArray)
		{
			return TypeName(type.GetElementType()) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
		}
		if (type.IsGenericType && !type.IsGenericTypeDefinition)
		{
			string arguments = string.Join(",", type.GetGenericArguments().Select(a => "[" + AssemblyQualifiedName(a) + "]"));
			return type.GetGenericTypeDefinition().FullName + "[" + arguments + "]";
		}
		return type.FullName;
	}

	public static string AssemblyName(Type type) => IsSystemType(type) ? Mscorlib : AssemblyOf(type).FullName;

	public static string AssemblyQualifiedName(Type type) => TypeName(type) + ", " + AssemblyName(type);

	private static Assembly AssemblyOf(Type type)
	{
		while (type.IsArray)
		{
			type = type.GetElementType();
		}
		return type.Assembly;
	}

	/// <summary>The MS-NRBF primitive code of a .NET type, or None when the type is not an NRBF primitive.</summary>
	public static NrbfPrimitiveType PrimitiveOf(Type type)
	{
		if (type.IsEnum)
		{
			return NrbfPrimitiveType.None; // enums are written as classes with a value__ member
		}
		switch (Type.GetTypeCode(type))
		{
			case TypeCode.Boolean: return NrbfPrimitiveType.Boolean;
			case TypeCode.Byte: return NrbfPrimitiveType.Byte;
			case TypeCode.Char: return NrbfPrimitiveType.Char;
			case TypeCode.Decimal: return NrbfPrimitiveType.Decimal;
			case TypeCode.Double: return NrbfPrimitiveType.Double;
			case TypeCode.Int16: return NrbfPrimitiveType.Int16;
			case TypeCode.Int32: return NrbfPrimitiveType.Int32;
			case TypeCode.Int64: return NrbfPrimitiveType.Int64;
			case TypeCode.SByte: return NrbfPrimitiveType.SByte;
			case TypeCode.Single: return NrbfPrimitiveType.Single;
			case TypeCode.DateTime: return NrbfPrimitiveType.DateTime;
			case TypeCode.UInt16: return NrbfPrimitiveType.UInt16;
			case TypeCode.UInt32: return NrbfPrimitiveType.UInt32;
			case TypeCode.UInt64: return NrbfPrimitiveType.UInt64;
		}
		return type == typeof(TimeSpan) ? NrbfPrimitiveType.TimeSpan : NrbfPrimitiveType.None;
	}
}
