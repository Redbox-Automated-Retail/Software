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
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return null;
		}
		if (m_stylesheets.ContainsKey(name))
		{
			LogHelper.Instance.Log("Stylesheet resource '{0}' is in cache.", name);
			return m_stylesheets[name];
		}
		LogHelper.Instance.Log("Stylesheet resource '{0}' is not in cache; build cache entry.", name);
		XmlNode xml = service.GetXml(name);
		if (xml == null)
		{
			return null;
		}
		StyleSheet styleSheet = new StyleSheet
		{
			Name = name
		};
		m_stylesheets[name] = styleSheet;
		XmlNodeList xmlNodeList = xml.SelectNodes("style");
		if (xmlNodeList != null)
		{
			foreach (XmlNode item in xmlNodeList)
			{
				string attributeValue = item.GetAttributeValue<string>("name");
				if (string.IsNullOrEmpty(attributeValue))
				{
					continue;
				}
				if (styleSheet.GetStyle(attributeValue) != null)
				{
					LogHelper.Instance.Log("WARNING: Duplicate style '{0}' defined in stylesheet resource: {1}.", attributeValue, name);
					continue;
				}
				XmlNodeList xmlNodeList2 = item.SelectNodes("state");
				if (xmlNodeList2 == null)
				{
					continue;
				}
				IStyleSheetStyle styleSheetStyle = styleSheet.AddStyle(attributeValue);
				foreach (XmlNode item2 in xmlNodeList2)
				{
					string attributeValue2 = item2.GetAttributeValue<string>("name");
					if (string.IsNullOrEmpty(attributeValue2))
					{
						continue;
					}
					if (styleSheetStyle.GetState(attributeValue2) != null)
					{
						LogHelper.Instance.Log("WARNING: Duplicate style state '{0}' defined in style '{1}' of resource: {2}.", attributeValue2, attributeValue, name);
						continue;
					}
					XmlNodeList xmlNodeList3 = item2.SelectNodes("property");
					if (xmlNodeList3 == null)
					{
						continue;
					}
					IStyleSheetState styleSheetState = styleSheetStyle.AddState(attributeValue2);
					foreach (XmlNode item3 in xmlNodeList3)
					{
						string attributeValue3 = item3.GetAttributeValue<string>("name");
						if (!string.IsNullOrEmpty(attributeValue3))
						{
							((StyleSheetState)styleSheetState)[attributeValue3] = item3.GetAttributeValue<object>("value");
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
