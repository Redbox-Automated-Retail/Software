using System.Collections.Specialized;

namespace Redbox.HAL.Core;

public class MacroContainer
{
	private StringDictionary m_values;

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

	public static MacroContainer Instance => Singleton<MacroContainer>.Instance;

	public string ExpandRuntimeMacros(string value)
	{
		return ExpandMacros(value, "${", "}");
	}

	public string ExpandConstantMacros(string value)
	{
		return ExpandMacros(value, "$(", ")");
	}

	internal string ExpandMacros(string value, string preamble, string postamble)
	{
		if (value == null)
		{
			return null;
		}
		while (true)
		{
			string macroName = StringExtensions.ExtractCodeFromBrackets(value, preamble, postamble);
			if (string.IsNullOrEmpty(macroName))
			{
				break;
			}
			string expansion = Values[macroName];
			if (expansion == null)
			{
				expansion = string.Empty;
			}
			value = value.Replace($"{preamble}{macroName}{postamble}", expansion);
		}
		return value;
	}

	private MacroContainer()
	{
	}
}
