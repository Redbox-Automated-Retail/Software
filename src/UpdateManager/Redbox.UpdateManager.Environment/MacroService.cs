using Redbox.Macros;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

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

	public string ExpandProperties(string input)
	{
		return m_properties.ExpandProperties(input, Location.UnknownLocation);
	}
}
