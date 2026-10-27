using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Services.Controllers;

public class BundleFunctions
{
	public static string GetDictionaryStringValue(string dictionaryName, string key)
	{
		string result = null;
		if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(dictionaryName))
		{
			Dictionary<string, object> dictionary = GetDictionary(dictionaryName);
			if (dictionary != null)
			{
				object value = null;
				if (dictionary.TryGetValue(key, out value))
				{
					result = value as string;
				}
			}
		}
		return result;
	}

	public static Dictionary<string, object> GetDictionary(string name)
	{
		XmlNode xml = ServiceLocator.Instance.GetService<IResourceBundleService>().GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = item2.GetAttributeValue<string>("name");
				object value = item2.GetNodeValue(string.Empty);
				string attributeValue2 = item2.GetAttributeValue<string>("type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = attributeValue2.ToType();
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
				dictionary[attributeValue] = value;
			}
		}
		return dictionary;
	}

	public static string GetCulture()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["culture"];
	}
}
