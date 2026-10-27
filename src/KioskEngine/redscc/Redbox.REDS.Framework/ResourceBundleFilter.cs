using System.Collections.Generic;

namespace Redbox.REDS.Framework;

internal class ResourceBundleFilter : IResourceBundleFilter
{
	private readonly IDictionary<string, string> m_filters = new Dictionary<string, string>();

	public string this[string key]
	{
		get
		{
			if (!m_filters.ContainsKey(key))
			{
				return null;
			}
			return m_filters[key];
		}
		set
		{
			m_filters[key] = value;
		}
	}
}
