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
		List<string> list = new List<string>();
		foreach (string key in Values.Keys)
		{
			list.Add(key);
		}
		List<string> list2 = new List<string>(exclusionList);
		foreach (string item in list)
		{
			string eachKey = item;
			if (!list2.Exists((string eachExclusion) => eachKey.StartsWith(eachExclusion, StringComparison.CurrentCultureIgnoreCase)))
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
			string text = Redbox_002EMacros1309_002EGlobalStringExtensions.ExtractCodeFromBrackets(value, preamble, postamble);
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			string text2 = ScriptFunction.ExecuteScriptFunction(text, out var functionExecuted);
			if (!functionExecuted)
			{
				text2 = Values[text];
			}
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			value = value.Replace($"{preamble}{text}{postamble}", text2);
		}
		return value;
	}

	private MacroContainer()
	{
	}
}
