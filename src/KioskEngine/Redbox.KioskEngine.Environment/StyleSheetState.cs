using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class StyleSheetState : IStyleSheetState
{
	private readonly IDictionary<string, object> m_properties = new Dictionary<string, object>();

	public string Name { get; internal set; }

	public object this[string key]
	{
		get
		{
			if (!m_properties.ContainsKey(key))
			{
				return null;
			}
			return m_properties[key];
		}
		internal set
		{
			m_properties[key] = value;
		}
	}
}
