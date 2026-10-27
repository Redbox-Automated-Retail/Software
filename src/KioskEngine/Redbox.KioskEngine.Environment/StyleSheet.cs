using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class StyleSheet : IStyleSheet
{
	private readonly IDictionary<string, IStyleSheetStyle> m_styles = new Dictionary<string, IStyleSheetStyle>();

	public string Name { get; internal set; }

	public void RemoveStyle(string name)
	{
		m_styles.Remove(name);
	}

	public IStyleSheetStyle AddStyle(string name)
	{
		if (m_styles.ContainsKey(name))
		{
			return m_styles[name];
		}
		StyleSheetStyle styleSheetStyle = new StyleSheetStyle
		{
			Name = name
		};
		m_styles[name] = styleSheetStyle;
		return styleSheetStyle;
	}

	public IStyleSheetStyle GetStyle(string name)
	{
		if (!m_styles.ContainsKey(name))
		{
			return null;
		}
		return m_styles[name];
	}
}
