using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class StyleSheetService : IStyleSheetService
{
	private readonly IDictionary<string, IStyleSheet> m_stylesheets = new Dictionary<string, IStyleSheet>();

	public static IStyleSheetService Instance => Singleton<StyleSheetService>.Instance;

	public ReadOnlyCollection<IStyleSheet> StyleSheets => new List<IStyleSheet>(m_stylesheets.Values).AsReadOnly();

	public void Reset()
	{
		LogHelper.Instance.Log("Style sheet service reset.");
		m_stylesheets.Clear();
	}

	public IStyleSheet New(string name)
	{
		IResourceBundleService resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (resourceBundleService == null)
		{
			return null;
		}
		if (m_stylesheets.ContainsKey(name))
		{
			LogHelper.Instance.Log("Stylesheet resource '{0}' is in cache.", name);
			return m_stylesheets[name];
		}
		LogHelper.Instance.Log("Stylesheet resource '{0}' is not in cache; build cache entry.", name);
		XmlNode xml = resourceBundleService.GetXml(name);
		if (xml == null)
		{
			return null;
		}
		StyleSheet styleSheet = new StyleSheet
		{
			Name = name
		};
		m_stylesheets[name] = styleSheet;
		XmlNodeList styleNodes = xml.SelectNodes("style");
		if (styleNodes != null)
		{
			foreach (XmlNode styleNode in styleNodes)
			{
				string styleName = styleNode.GetAttributeValue<string>("name");
				if (string.IsNullOrEmpty(styleName))
				{
					continue;
				}
				if (styleSheet.GetStyle(styleName) != null)
				{
					LogHelper.Instance.Log("WARNING: Duplicate style '{0}' defined in stylesheet resource: {1}.", styleName, name);
					continue;
				}
				XmlNodeList stateNodes = styleNode.SelectNodes("state");
				if (stateNodes == null)
				{
					continue;
				}
				IStyleSheetStyle styleSheetStyle = styleSheet.AddStyle(styleName);
				foreach (XmlNode stateNode in stateNodes)
				{
					string stateName = stateNode.GetAttributeValue<string>("name");
					if (string.IsNullOrEmpty(stateName))
					{
						continue;
					}
					if (styleSheetStyle.GetState(stateName) != null)
					{
						LogHelper.Instance.Log("WARNING: Duplicate style state '{0}' defined in style '{1}' of resource: {2}.", stateName, styleName, name);
						continue;
					}
					XmlNodeList propertyNodes = stateNode.SelectNodes("property");
					if (propertyNodes == null)
					{
						continue;
					}
					IStyleSheetState styleSheetState = styleSheetStyle.AddState(stateName);
					foreach (XmlNode propertyNode in propertyNodes)
					{
						string propertyName = propertyNode.GetAttributeValue<string>("name");
						if (!string.IsNullOrEmpty(propertyName))
						{
							((StyleSheetState)styleSheetState)[propertyName] = propertyNode.GetAttributeValue<object>("value");
						}
					}
				}
			}
		}
		return styleSheet;
	}

	public IStyleSheet GetStyleSheet(string name)
	{
		if (!m_stylesheets.ContainsKey(name))
		{
			return null;
		}
		return m_stylesheets[name];
	}

	private StyleSheetService()
	{
	}
}
