using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using Redbox.Core;

namespace Redbox.Macros;

public sealed class FunctionFactory
{
	private delegate Attribute ConstructDelegate(Attribute attribute);

	private static readonly Hashtable m_methodInfoCollection = new Hashtable();

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static bool ScanAssembly(string assemblyFile)
	{
		return ScanAssembly(Assembly.LoadFrom(assemblyFile));
	}

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static bool ScanAssembly(Assembly assembly)
	{
		bool foundAny = false;
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			bool found = ScanTypeForFunctions(types[i]);
			foundAny |= found;
		}
		return foundAny;
	}

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static void ScanDir(string path, bool failOnError)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		List<FileInfo> fileInfoList = new List<FileInfo>(directoryInfo.GetFiles("*.dll"));
		fileInfoList.AddRange(directoryInfo.GetFiles("*.exe"));
		fileInfoList.ForEach(delegate(FileInfo f)
		{
#if !NETFRAMEWORK
			if (!NetFrameworkCompatibility.IsNetAssembly(f.FullName))
			{
				return;
			}
#endif
			try
			{
				ScanAssembly(f.FullName);
			}
			catch (Exception innerException)
			{
				string text = string.Format(CultureInfo.InvariantCulture, "Failure scanning \"{0}\" for extensions", f);
				if (failOnError)
				{
					throw new EvaluatorException(text + ".", Location.UnknownLocation, innerException);
				}
			}
		});
	}

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static void ScanExecutionPath()
	{
		ScanDir(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)!.Replace("file:\\", ""), failOnError: true);
	}

	internal static MethodInfo LookupFunction(string functionName, FunctionArgument[] args)
	{
		object? registered = m_methodInfoCollection[functionName];
		if (registered == null)
		{
			throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1052"), functionName));
		}
		MethodInfo? methodInfo = registered as MethodInfo;
		if (methodInfo != null)
		{
			if (methodInfo.GetParameters().Length == args.Length)
			{
				CheckDeprecation(functionName, methodInfo);
				return methodInfo;
			}
		}
		else
		{
			ArrayList arrayList = (ArrayList)registered;
			for (int i = 0; i < arrayList.Count; i++)
			{
				methodInfo = (MethodInfo)arrayList[i]!;
				if (methodInfo.GetParameters().Length == args.Length)
				{
					CheckDeprecation(functionName, methodInfo);
					return methodInfo;
				}
			}
		}
		throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1044"), functionName, args.Length));
	}

	private static void CheckDeprecation(string functionName, MemberInfo function)
	{
		ObsoleteAttribute? obsoleteAttribute = ((ObsoleteAttribute?)Attribute.GetCustomAttribute(function, typeof(ObsoleteAttribute), inherit: true)) ?? ((ObsoleteAttribute?)Attribute.GetCustomAttribute(function.DeclaringType!, typeof(ObsoleteAttribute), inherit: true));
		if (obsoleteAttribute != null)
		{
			string text = string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1087"), functionName, obsoleteAttribute.Message);
			if (obsoleteAttribute.IsError)
			{
				throw new EvaluatorException(text, Location.UnknownLocation);
			}
			LogHelper.Instance.Log($"{text}", LogEntryType.Info);
		}
	}

	private static bool ScanTypeForFunctions(Type type)
	{
		try
		{
			FunctionSetAttribute? functionSetAttribute = (FunctionSetAttribute?)ExtrapolateAttribute(type, "Redbox.Macros.FunctionSetAttribute", FunctionSetAttributeConstructor);
			if (functionSetAttribute == null)
			{
				return false;
			}
			bool isFunctionSet = type.IsTypeOfExpressionEvaluator();
			if (type.IsSubclassOfFunctionSetBase() && !type.IsAbstract)
			{
				isFunctionSet = true;
			}
			if (isFunctionSet)
			{
				string prefix = functionSetAttribute.Prefix;
				if (!string.IsNullOrEmpty(prefix))
				{
					prefix += "::";
					MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
					foreach (MethodInfo methodInfo in methods)
					{
						FunctionAttribute? functionAttribute = (FunctionAttribute?)ExtrapolateAttribute(methodInfo, "Redbox.Macros.FunctionAttribute", FunctionAttributeConstructor);
						if (functionAttribute != null)
						{
							RegisterFunction(prefix + functionAttribute.Name, methodInfo);
						}
					}
					return true;
				}
				LogHelper.Instance.Log($"Ignoring functions in type \"{type.AssemblyQualifiedName}\": no prefix was set.", LogEntryType.Info);
				return false;
			}
			return false;
		}
		catch
		{
			LogHelper.Instance.Log($"Failure scanning \"{type.AssemblyQualifiedName}\" for functions.", LogEntryType.Error);
			throw;
		}
	}

	private static void RegisterFunction(string key, MethodInfo info)
	{
		object? registered = m_methodInfoCollection[key];
		if (registered == null)
		{
			m_methodInfoCollection.Add(key, info);
			return;
		}
		MethodInfo? methodInfo = registered as MethodInfo;
		if (methodInfo == null)
		{
			((ArrayList)registered).Add(info);
			return;
		}
		ArrayList value = new ArrayList(3) { methodInfo, info };
		m_methodInfoCollection[key] = value;
	}

	private static Attribute? ExtrapolateAttribute(MemberInfo type, string fullname, ConstructDelegate doconstruct)
	{
		Attribute[] customAttributes = Attribute.GetCustomAttributes(type);
		foreach (Attribute attribute in customAttributes)
		{
			Type? type2 = attribute.TypeId as Type;
			if (type2 != null && type2.FullName == fullname)
			{
				return doconstruct(attribute);
			}
		}
		return null;
	}

	private static Attribute FunctionSetAttributeConstructor(Attribute attribute)
	{
		return new FunctionSetAttribute(attribute.GetType().InvokeMember("Prefix", BindingFlags.GetProperty, null, attribute, new object[0])!.ToString()!, attribute.GetType().InvokeMember("Category", BindingFlags.GetProperty, null, attribute, new object[0])!.ToString()!);
	}

	private static Attribute FunctionAttributeConstructor(Attribute attribute)
	{
		return new FunctionAttribute(attribute.GetType().InvokeMember("Name", BindingFlags.GetProperty, null, attribute, new object[0])!.ToString()!);
	}
}
