using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.Core;
using Redbox.Extensibility.ComponentModel;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

public class NameValueProperty
{
	private List<string> m_displayValues;

	private IDictionary<string, NameValueProperty> m_properties;

	public Type Type { get; set; }

	public string Name { get; set; }

	public object Value { get; set; }

	public bool ReadOnly { get; set; }

	public string Comment { get; set; }

	public string Category { get; set; }

	public bool IsRequired { get; set; }

	public string DisplayName { get; set; }

	public string TargetMacro { get; set; }

	public object DefaultValue { get; set; }

	public PropertyEditor? Editor { get; set; }

	public List<string> DisplayValues
	{
		get
		{
			if (m_displayValues == null)
			{
				m_displayValues = new List<string>();
			}
			return m_displayValues;
		}
	}

	public IDictionary<string, NameValueProperty> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, NameValueProperty>();
			}
			return m_properties;
		}
	}

	public static IDictionary<string, NameValueProperty> ParseConfigurationXml(string xml, ErrorList errors)
	{
		Dictionary<string, NameValueProperty> dictionary = new Dictionary<string, NameValueProperty>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		if (xmlDocument.DocumentElement == null)
		{
			return dictionary;
		}
		XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("property");
		if (xmlNodeList != null)
		{
			WalkPropertyTree(xmlNodeList, errors, dictionary);
		}
		return dictionary;
	}

	public object GetTypedValue()
	{
		object obj = Value;
		if (obj != null)
		{
			obj = MacroContainer.Instance.ExpandRuntimeMacros(obj.ToString());
		}
		if (Type == null)
		{
			return obj;
		}
		return ConversionHelper.ChangeType(obj, Type);
	}

	public object GetTypedDefaultValue()
	{
		object obj = DefaultValue;
		if (obj != null)
		{
			obj = MacroContainer.Instance.ExpandRuntimeMacros(obj.ToString());
		}
		if (Type == null)
		{
			return obj;
		}
		return ConversionHelper.ChangeType(obj, Type);
	}

	internal static void WalkPropertyTree(XmlNodeList nodes, ErrorList errors, IDictionary<string, NameValueProperty> properties)
	{
		foreach (XmlNode node2 in nodes)
		{
			NameValueProperty nameValueProperty = new NameValueProperty();
			nameValueProperty.Name = node2.GetExpandedAttributeValue<string>("name");
			nameValueProperty.Value = node2.GetExpandedAttributeValue<string>("value");
			nameValueProperty.Comment = node2.GetExpandedAttributeValue<string>("comment");
			nameValueProperty.ReadOnly = node2.GetExpandedAttributeValue<bool>("read-only");
			nameValueProperty.IsRequired = node2.GetExpandedAttributeValue<bool>("is-required");
			nameValueProperty.Category = node2.GetExpandedAttributeValue<string>("category");
			nameValueProperty.TargetMacro = node2.GetExpandedAttributeValue<string>("target");
			nameValueProperty.Editor = node2.GetExpandedAttributeValue<PropertyEditor?>("editor");
			nameValueProperty.DisplayName = node2.GetExpandedAttributeValue<string>("display-name");
			nameValueProperty.DefaultValue = node2.GetExpandedAttributeValue<string>("default-value");
			NameValueProperty nameValueProperty2 = nameValueProperty;
			string expandedAttributeValue = node2.GetExpandedAttributeValue<string>("type");
			if (!string.IsNullOrEmpty(expandedAttributeValue))
			{
				try
				{
					nameValueProperty2.Type = StringExtensions.ToType(expandedAttributeValue);
				}
				catch (Exception e)
				{
					errors.Add(Error.NewError("C003", "An invalid .NET type was specified.", e));
					continue;
				}
			}
			if (string.IsNullOrEmpty(nameValueProperty2.Name))
			{
				errors.Add(Error.NewError("C003", "The property node must have a valid name attribute.", "Specify a valid Name attribute."));
				continue;
			}
			if (string.IsNullOrEmpty(nameValueProperty2.Category))
			{
				nameValueProperty2.Category = "Miscellaneous";
			}
			XmlNodeList xmlNodeList = node2.SelectNodes("display-value");
			if (xmlNodeList != null)
			{
				foreach (XmlNode item in xmlNodeList)
				{
					string expandedAttributeValue2 = item.GetExpandedAttributeValue<string>("text");
					if (!string.IsNullOrEmpty(expandedAttributeValue2))
					{
						nameValueProperty2.DisplayValues.Add(expandedAttributeValue2);
					}
				}
			}
			XmlNodeList xmlNodeList2 = node2.SelectNodes("property");
			if (xmlNodeList2 != null)
			{
				WalkPropertyTree(xmlNodeList2, errors, nameValueProperty2.Properties);
			}
			properties.Add(nameValueProperty2.Name, nameValueProperty2);
		}
	}
}
