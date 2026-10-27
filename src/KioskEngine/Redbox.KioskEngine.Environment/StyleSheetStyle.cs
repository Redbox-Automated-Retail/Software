using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class StyleSheetStyle : IStyleSheetStyle
{
	private readonly IDictionary<string, IStyleSheetState> m_states = new Dictionary<string, IStyleSheetState>();

	public string Name { get; internal set; }

	public void RemoveState(string name)
	{
		m_states.Remove(name);
	}

	public IStyleSheetState AddState(string name)
	{
		if (m_states.ContainsKey(name))
		{
			return m_states[name];
		}
		StyleSheetState styleSheetState = new StyleSheetState
		{
			Name = name
		};
		m_states[name] = styleSheetState;
		return styleSheetState;
	}

	public IStyleSheetState GetState(string name)
	{
		if (!m_states.ContainsKey(name))
		{
			return null;
		}
		return m_states[name];
	}
}
