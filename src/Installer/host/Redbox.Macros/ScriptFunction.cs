using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.Tokenizer.Framework;

namespace Redbox.Macros;

public static class ScriptFunction
{
	public static string ExecuteScriptFunction(string value, out bool functionExecuted)
	{
		functionExecuted = false;
		ScriptFunctionTokenizer scriptFunctionTokenizer = new ScriptFunctionTokenizer(1, value);
		scriptFunctionTokenizer.Tokenize();
		SimpleToken mnemonic = scriptFunctionTokenizer.Tokens.GetMnemonic();
		if (mnemonic == null)
		{
			return value;
		}
		MethodInfo scriptFunction = ScriptFunctionAttribute.GetScriptFunction(mnemonic.Value, typeof(ScriptFunction));
		if (scriptFunction != null)
		{
			ParameterInfo[] parameters = scriptFunction.GetParameters();
			List<object> list = new List<object>();
			int num = 0;
			foreach (SimpleToken symbol in scriptFunctionTokenizer.Tokens.GetSymbols())
			{
				list.Add(Convert.ChangeType(MacroContainer.Instance.ExpandRuntimeMacros(symbol.Value), parameters[num++].ParameterType));
			}
			if (list.Count < parameters.Length)
			{
				return value;
			}
			value = scriptFunction.Invoke(null, list.ToArray()).ToString();
			functionExecuted = true;
		}
		return value;
	}

	[ScriptFunction("string::remove")]
	public static string RemoveString(string source, string toRemove)
	{
		if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toRemove))
		{
			return string.Empty;
		}
		int index = source.IndexOf(toRemove);
		if (index < 0)
		{
			return source;
		}
		return source.Remove(index, toRemove.Length);
	}

	[ScriptFunction("string::remove-at")]
	public static string RemoveStringAt(string source, int index)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}
		if (index != -1)
		{
			return source.Substring(0, index);
		}
		return source;
	}

	[ScriptFunction("string::index-of")]
	public static int IndexOf(string source, string toFind)
	{
		if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toFind))
		{
			return -1;
		}
		return source.IndexOf(toFind);
	}

	[ScriptFunction("string::last-index-of")]
	public static int LastIndexOf(string source, string toFind)
	{
		if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toFind))
		{
			return -1;
		}
		return source.LastIndexOf(toFind);
	}

	[ScriptFunction("string::get-length")]
	public static int GetStringLength(string source)
	{
		return source?.Length ?? 0;
	}

	[ScriptFunction("string::to-upper")]
	public static string MakeUppercase(string source)
	{
		if (source != null)
		{
			return source.ToUpper();
		}
		return string.Empty;
	}

	[ScriptFunction("string::to-lower")]
	public static string MakeLowercase(string source)
	{
		if (source != null)
		{
			return source.ToLower();
		}
		return string.Empty;
	}
}
