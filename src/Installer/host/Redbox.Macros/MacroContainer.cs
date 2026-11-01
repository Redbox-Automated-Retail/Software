using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Redbox.Core;

namespace Redbox.Macros;

public class MacroContainer
{
	private StringDictionary m_values;

	public static MacroContainer Instance => Redbox_002EMacros1309_002ESingleton<MacroContainer>.Instance;

	public StringDictionary Values
	{
		get
		{
			if (m_values == null)
			{
				m_values = new StringDictionary();
			}
			return m_values;
		}
	}

	public string ExpandRuntimeMacros(string value)
	{
		return ExpandMacros(value, "${", "}");
	}

	public string ExpandConstantMacros(string value)
	{
		return ExpandMacros(value, "$(", ")");
	}

	public void Clear(params string[] exclusionList)
	{
		List<string> keys = new List<string>();
		foreach (string key in Values.Keys)
		{
			keys.Add(key);
		}
		List<string> exclusions = new List<string>(exclusionList);
		foreach (string key in keys)
		{
			string eachKey = key;
			if (!exclusions.Exists((string eachExclusion) => eachKey.StartsWith(eachExclusion, StringComparison.CurrentCultureIgnoreCase)))
			{
				Values.Remove(eachKey);
			}
		}
	}

	public void Clear()
	{
		Values.Clear();
	}

	internal string ExpandMacros(string value, string preamble, string postamble)
	{
		if (value == null)
		{
			return null;
		}
		while (true)
		{
			string code = Redbox_002EMacros1309_002EGlobalStringExtensions.ExtractCodeFromBrackets(value, preamble, postamble);
			if (string.IsNullOrEmpty(code))
			{
				break;
			}
			string replacement = ScriptFunction.ExecuteScriptFunction(code, out var functionExecuted);
			if (!functionExecuted)
			{
				replacement = Values[code];
			}
			if (replacement == null)
			{
				replacement = string.Empty;
			}
			value = value.Replace($"{preamble}{code}{postamble}", replacement);
		}
		return value;
	}

	private MacroContainer()
	{
	}
}
