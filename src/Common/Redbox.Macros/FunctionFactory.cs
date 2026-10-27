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
		bool flag = false;
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			bool flag2 = ScanTypeForFunctions(types[i]);
			flag |= flag2;
		}
		return flag;
	}

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static void ScanDir(string path, bool failOnError)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		List<FileInfo> list = new List<FileInfo>(directoryInfo.GetFiles("*.dll"));
		list.AddRange(directoryInfo.GetFiles("*.exe"));
		list.ForEach(delegate(FileInfo f)
		{
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
		ScanDir(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", ""), failOnError: true);
	}

	internal static MethodInfo LookupFunction(string functionName, FunctionArgument[] args)
	{
		object obj = m_methodInfoCollection[functionName];
		if (obj == null)
		{
			throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1052"), functionName));
		}
		MethodInfo methodInfo = obj as MethodInfo;
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
			ArrayList arrayList = (ArrayList)obj;
			for (int i = 0; i < arrayList.Count; i++)
			{
				methodInfo = (MethodInfo)arrayList[i];
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
		ObsoleteAttribute obsoleteAttribute = ((ObsoleteAttribute)Attribute.GetCustomAttribute(function, typeof(ObsoleteAttribute), inherit: true)) ?? ((ObsoleteAttribute)Attribute.GetCustomAttribute(function.DeclaringType, typeof(ObsoleteAttribute), inherit: true));
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
			FunctionSetAttribute functionSetAttribute = (FunctionSetAttribute)ExtrapolateAttribute(type, "Redbox.Macros.FunctionSetAttribute", FunctionSetAttributeConstructor);
			if (functionSetAttribute == null)
			{
				return false;
			}
			bool flag = type.IsTypeOfExpressionEvaluator();
			if (type.IsSubclassOfFunctionSetBase() && !type.IsAbstract)
			{
				flag = true;
			}
			if (flag)
			{
				string prefix = functionSetAttribute.Prefix;
				if (!string.IsNullOrEmpty(prefix))
				{
					prefix += "::";
					MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
					foreach (MethodInfo methodInfo in methods)
					{
						FunctionAttribute functionAttribute = (FunctionAttribute)ExtrapolateAttribute(methodInfo, "Redbox.Macros.FunctionAttribute", FunctionAttributeConstructor);
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
		object obj = m_methodInfoCollection[key];
		if (obj == null)
		{
			m_methodInfoCollection.Add(key, info);
			return;
		}
		MethodInfo methodInfo = obj as MethodInfo;
		if (methodInfo == null)
		{
			((ArrayList)obj).Add(info);
			return;
		}
		ArrayList value = new ArrayList(3) { methodInfo, info };
		m_methodInfoCollection[key] = value;
	}

	private static Attribute ExtrapolateAttribute(MemberInfo type, string fullname, ConstructDelegate doconstruct)
	{
		Attribute[] customAttributes = Attribute.GetCustomAttributes(type);
		foreach (Attribute attribute in customAttributes)
		{
			Type type2 = attribute.TypeId as Type;
			if (type2 != null && type2.FullName == fullname)
			{
				return doconstruct(attribute);
			}
		}
		return null;
	}

	private static Attribute FunctionSetAttributeConstructor(Attribute attribute)
	{
		return new FunctionSetAttribute(attribute.GetType().InvokeMember("Prefix", BindingFlags.GetProperty, null, attribute, new object[0]).ToString(), attribute.GetType().InvokeMember("Category", BindingFlags.GetProperty, null, attribute, new object[0]).ToString());
	}

	private static Attribute FunctionAttributeConstructor(Attribute attribute)
	{
		return new FunctionAttribute(attribute.GetType().InvokeMember("Name", BindingFlags.GetProperty, null, attribute, new object[0]).ToString());
	}
}
