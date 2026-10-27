using System;
using System.Xml;

namespace Redbox.HAL.Component.Model.Extensions;

public static class XmlNodeExtensions
{
	public static void SelectSingleNodeAndSetValue<T>(this XmlNode node, string path, T value)
	{
		GetTargetNode(node, path).InnerText = value.ToString();
	}

	public static void SelectSingleNodeAndSetInnerXml<T>(this XmlNode node, string path, T instance)
	{
		GetTargetNode(node, path).InnerXml = instance.ToString();
	}

	public static T GetNodeValue<T>(this XmlNode node, T defaultValue)
	{
		try
		{
			if (node != null && !string.IsNullOrEmpty(node.InnerText))
			{
				return ConversionHelper.ChangeType<T>(ServiceLocator.Instance.GetService<IRuntimeService>().ExpandConstantMacros(node.InnerText));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}

	public static T GetAttributeValue<T>(this XmlNode node, string attributeName)
	{
		return node.GetAttributeValue(attributeName, default(T));
	}

	public static T GetAttributeValue<T>(this XmlNode node, string attributeName, T defaultValue)
	{
		try
		{
			if (node != null && node.Attributes[attributeName] != null)
			{
				return ConversionHelper.ChangeType<T>(ServiceLocator.Instance.GetService<IRuntimeService>().ExpandConstantMacros(node.Attributes[attributeName].Value));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}

	public static void SetAttributeValue<T>(this XmlNode node, string attributeName, T value)
	{
		XmlAttribute xmlAttribute = node.Attributes[attributeName];
		if (xmlAttribute != null && !typeof(T).IsValueType && value == null)
		{
			node.Attributes.Remove(xmlAttribute);
			return;
		}
		if (xmlAttribute == null)
		{
			xmlAttribute = node.OwnerDocument.CreateAttribute(attributeName);
			node.Attributes.Append(xmlAttribute);
		}
		xmlAttribute.Value = value.ToString();
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
