using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class ConfigItem
{
	private object m_value;

	public bool ReadOnly { get; private set; }

	public string Name { get; private set; }

	public object Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
			Node.SetAttributeValue("value", value.ToString());
		}
	}

	public string Description { get; private set; }

	public object DefaultValue { get; set; }

	public string DisplayName { get; set; }

	public string CategoryName { get; private set; }

	public List<string> ValidValues { get; private set; }

	public int ValidValuesCount { get; private set; }

	public string CustomEditor { get; private set; }

	public Type ItemType { get; private set; }

	public XmlNode Node { get; private set; }

	public ConfigItem(XmlNode node)
	{
		Node = node;
		CustomEditor = node.GetAttributeValue<string>("custom-editor");
		string attributeValue = node.GetAttributeValue("type", typeof(string).ToString());
		ItemType = Type.GetType(attributeValue);
		ReadOnly = node.GetAttributeValue("read-only", defaultValue: false);
		Name = node.GetAttributeValue<string>("name");
		m_value = ((CustomEditor != null) ? CustomEditor : node.GetAttributeValue<string>("value"));
		DefaultValue = m_value;
		Description = node.GetAttributeValue<string>("description");
		DisplayName = node.GetAttributeValue<string>("display-name");
		CategoryName = node.GetAttributeValue<string>("category");
		ValidValuesCount = node.GetAttributeValue("valid-value-count", 0);
		if (ValidValuesCount <= 0)
		{
			return;
		}
		ValidValues = new List<string>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string attributeValue2 = childNode.GetAttributeValue<string>("value");
			if (attributeValue2 != null)
			{
				ValidValues.Add(attributeValue2);
			}
		}
		if (Value != null && !ValidValues.Contains(Value.ToString()))
		{
			ValidValues.Add(Value.ToString());
		}
		ItemType = ValidValues.GetType();
	}
}
