using System;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public static class XmlNodeExtensions
{
	public static void SelectSingleNodeAndSetInnerXml<T>(XmlNode node, string path, T instance)
	{
		GetTargetNode(node, path).InnerXml = instance.ToString();
	}

	public static void SelectSingleNodeAndSetValue<T>(XmlNode node, string path, T value)
	{
		GetTargetNode(node, path).InnerText = value.ToString();
	}

	public static T GetAttributeValue<T>(XmlNode node, string attributeName, T defaultValue)
	{
		try
		{
			IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
			if (node != null && node.Attributes[attributeName] != null)
			{
				return (T)ConversionHelper.ChangeType(service.ExpandConstantMacros(node.Attributes[attributeName].Value), typeof(T));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}

	public static T GetNodeValue2<T>(XmlNode node, T defaultValue)
	{
		try
		{
			IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
			if (node != null && !string.IsNullOrEmpty(node.InnerText))
			{
				return (T)ConversionHelper.ChangeType(service.ExpandConstantMacros(node.InnerText), typeof(T));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}

	private static XmlNode GetTargetNode(XmlNode node, string path)
	{
		string[] array = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		XmlNode xmlNode = node;
		string[] array2 = array;
		foreach (string text in array2)
		{
			XmlNode xmlNode2 = xmlNode.SelectSingleNode(text);
			if (xmlNode2 == null)
			{
				xmlNode2 = xmlNode.OwnerDocument.CreateElement(text);
				xmlNode.AppendChild(xmlNode2);
			}
			xmlNode = xmlNode2;
		}
		return xmlNode;
	}
}
