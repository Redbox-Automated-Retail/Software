using System;
using System.Reflection;

namespace Redbox.Macros;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ScriptFunctionAttribute : Attribute
{
	public string FunctionName { get; set; }

	public ScriptFunctionAttribute(string functionName)
	{
		FunctionName = functionName;
	}

	public static MethodInfo GetScriptFunction(string functionName, Type type)
	{
		MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach (MethodInfo methodInfo in methods)
		{
			ScriptFunctionAttribute scriptFunctionAttribute = (ScriptFunctionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(ScriptFunctionAttribute));
			if (scriptFunctionAttribute != null && scriptFunctionAttribute.FunctionName == functionName)
			{
				return methodInfo;
			}
		}
		return null;
	}
}
