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
			string text = StringExtensions.ExtractCodeFromBrackets(value, preamble, postamble);
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			string text2 = Values[text];
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
