using System;
using System.Xml;
using Redbox.Core;

namespace Redbox.Macros;

public static class XmlNodeExtensions
{
	public static T GetExpandedAttributeValue<T>(this XmlNode node, string attributeName)
	{
		return node.GetExpandedAttributeValue(attributeName, default(T));
	}

	public static T GetExpandedAttributeValue<T>(this XmlNode node, string attributeName, T defaultValue)
	{
		try
		{
			if (node != null && node.Attributes[attributeName] != null)
			{
				return (T)Redbox_002EMacros1309_002EConversionHelper.ChangeType(MacroContainer.Instance.ExpandConstantMacros(node.Attributes[attributeName].Value), typeof(T));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}

	public static T GetExpandedNodeValue<T>(this XmlNode node)
	{
		return node.GetExpandedNodeValue(default(T));
	}

	public static T GetExpandedNodeValue<T>(this XmlNode node, T defaultValue)
	{
		try
		{
			if (node != null && !string.IsNullOrEmpty(node.InnerText))
			{
				return (T)Redbox_002EMacros1309_002EConversionHelper.ChangeType(MacroContainer.Instance.ExpandConstantMacros(node.InnerText), typeof(T));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}
}
