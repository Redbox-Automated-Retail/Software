using System;
using System.Reflection;

namespace Redbox.Macros.Functions;

[FunctionSet("assembly", "Assembly")]
public class AssemblyFunctions : FunctionSetBase
{
	public AssemblyFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("load-from-file")]
	public Assembly LoadFromFile(string assemblyFile)
	{
		return Assembly.LoadFrom(assemblyFile, AppDomain.CurrentDomain.Evidence);
	}

	[Function("load")]
	public Assembly Load(string assemblyString)
	{
		return Assembly.Load(assemblyString, AppDomain.CurrentDomain.Evidence);
	}

	[Function("get-full-name")]
	public static string GetFullName(Assembly assembly)
	{
		return assembly.FullName;
	}

	[Function("get-name")]
	public static AssemblyName GetName(Assembly assembly)
	{
		return assembly.GetName(copiedName: false);
	}

	[Function("get-location")]
	public static string GetLocation(Assembly assembly)
	{
		return assembly.Location;
	}
}
