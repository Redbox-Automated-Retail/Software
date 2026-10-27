using System;
using System.Reflection;

namespace Redbox.Macros.Functions;

[FunctionSet("assemblyname", "Assembly")]
public class AssemblyNameFunctions : FunctionSetBase
{
	public AssemblyNameFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-codebase")]
	public static string GetCodeBase(AssemblyName assemblyName)
	{
		return assemblyName.CodeBase;
	}

	[Function("get-escaped-codebase")]
	public static string GetEscapedCodeBase(AssemblyName assemblyName)
	{
		return assemblyName.EscapedCodeBase;
	}

	[Function("get-full-name")]
	public static string GetFullName(AssemblyName assemblyName)
	{
		return assemblyName.FullName;
	}

	[Function("get-name")]
	public static string GetName(AssemblyName assemblyName)
	{
		return assemblyName.Name;
	}

	[Function("get-version")]
	public static Version GetVersion(AssemblyName assemblyName)
	{
		return assemblyName.Version;
	}
}
