using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Macros;

namespace Redbox.KioskEngine.Environment;

public class MacroService : IMacroService
{
	private readonly PropertyDictionary m_properties = new PropertyDictionary();

	public string this[string name]
	{
		get
		{
			return m_properties[name];
		}
		set
		{
			m_properties[name] = value;
		}
	}

	public ICollection Keys => m_properties.Keys;

	public ICollection Values => m_properties.Values;

	public void Clear(ICollection<string> exclusions)
	{
		if (exclusions.Count == 0)
		{
			m_properties.Clear();
			return;
		}
		List<string> list = new List<string>();
		foreach (object key in m_properties.Keys)
		{
			if (!exclusions.Contains((string)key))
			{
				list.Add((string)key);
			}
		}
		foreach (string item in list)
		{
			m_properties.Remove(item);
		}
	}

	public string ExpandProperties(string input)
	{
		return m_properties.ExpandProperties(input, Location.UnknownLocation);
	}

	static MacroService()
	{
		FunctionFactory.ScanDir(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), failOnError: false);
	}
}
