using System;
using System.Xml;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EXmlNodeExtensions
{
	public static void SelectSingleNodeAndSetValue<T>(this XmlNode node, string path, T value)
	{
		GetTargetNode(node, path).InnerText = value.ToString();
	}

	public static void SelectSingleNodeAndSetInnerXml<T>(this XmlNode node, string path, T instance)
	{
		GetTargetNode(node, path).InnerXml = instance.ToString();
	}

	public static string SelectSingleNodeAndGetInnerText(this XmlNode node, string path)
	{
		XmlNode xmlNode = node.SelectSingleNode(path);
		if (xmlNode != null)
		{
			return GetNodeInnerText(xmlNode);
		}
		return null;
	}

	public static string GetNodeInnerXml(this XmlNode node)
	{
		return node.InnerXml;
	}

	public static string GetNodeInnerText(this XmlNode node)
	{
		return node.InnerText;
	}

	public static T GetNodeValue<T>(this XmlNode node)
	{
		return GetNodeValue(node, default(T));
	}

	public static T GetAttributeValue<T>(this XmlNode node, string attributeName)
	{
		return GetAttributeValue(node, attributeName, default(T));
	}

	public static T GetAttributeValue<T>(this XmlNode node, string attributeName, T defaultValue)
	{
		try
		{
			if (node != null && node.Attributes[attributeName] != null)
			{
				return (T)Redbox_002EMacros1309_002EConversionHelper.ChangeType(node.Attributes[attributeName].Value, typeof(T));
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

	public static T GetNodeValue<T>(this XmlNode node, T defaultValue)
	{
		try
		{
			if (node != null && !string.IsNullOrEmpty(node.InnerText))
			{
				return (T)Redbox_002EMacros1309_002EConversionHelper.ChangeType(node.InnerText, typeof(T));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}

	private static XmlNode GetTargetNode(XmlNode node, string path)
	{
		string[] segments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		XmlNode current = node;
		foreach (string segment in segments)
		{
			XmlNode child = current.SelectSingleNode(segment);
			if (child == null)
			{
				child = current.OwnerDocument.CreateElement(segment);
				current.AppendChild(child);
			}
			current = child;
		}
		return current;
	}
}
