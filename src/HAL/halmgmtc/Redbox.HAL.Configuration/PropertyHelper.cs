using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Configuration;

public static class PropertyHelper
{
	public static string FormatObjectAsXml(object instance, string rootName)
	{
		using StringWriter stringWriter = new StringWriter();
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
		xmlTextWriter.WriteStartDocument();
		xmlTextWriter.WriteStartElement(rootName);
		WalkObjectTree(instance, xmlTextWriter);
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
		xmlTextWriter.Flush();
		return stringWriter.ToString();
	}

	public static void UpdateObjectFromXml(object instance, XmlNode parentNode)
	{
		if (instance == null || parentNode == null)
		{
			return;
		}
		XmlNodeList propertyNodes = parentNode.SelectNodes("property");
		if (propertyNodes == null)
		{
			return;
		}
		foreach (XmlNode propertyNode in propertyNodes)
		{
			try
			{
				if (propertyNode.Attributes["read-only"] != null && string.Compare(propertyNode.Attributes["read-only"].Value, bool.TrueString, ignoreCase: true) == 0)
				{
					continue;
				}
				PropertyInfo propertyInfo = null;
				if (propertyNode.Attributes["name"] != null)
				{
					propertyInfo = instance.GetType().GetProperty(propertyNode.Attributes["name"].Value, BindingFlags.Instance | BindingFlags.Public);
				}
				if (propertyInfo == null)
				{
					continue;
				}
				XmlNodeList childPropertyNodes = propertyNode.SelectNodes("property");
				if (childPropertyNodes != null && childPropertyNodes.Count > 0)
				{
					UpdateObjectFromXml(propertyInfo.GetValue(instance, new object[0]), propertyNode);
					continue;
				}
				CustomEditorAttribute customEditorAttribute = (CustomEditorAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CustomEditorAttribute));
				if (propertyNode.ChildNodes.Count > 0 && customEditorAttribute != null && customEditorAttribute.SetMethodName != null)
				{
					instance.GetType().GetMethod(customEditorAttribute.SetMethodName, BindingFlags.Instance | BindingFlags.Public)?.Invoke(instance, new object[1] { propertyNode });
				}
				else if (propertyNode.Attributes["value"] != null)
				{
					string value = propertyNode.Attributes["value"].Value;
					propertyInfo.SetValue(instance, ConversionHelper.ChangeType(value, propertyInfo.PropertyType), null);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("UpdateXml caught exception", e);
			}
		}
	}

	private static void WalkObjectTree(object instance, XmlWriter xmlWriter)
	{
		if (instance == null)
		{
			return;
		}
		PropertyInfo[] properties = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			BrowsableAttribute browsableAttribute = (BrowsableAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(BrowsableAttribute));
			if (browsableAttribute != null && !browsableAttribute.Browsable)
			{
				continue;
			}
			xmlWriter.WriteStartElement("property");
			xmlWriter.WriteAttributeString("name", propertyInfo.Name);
			DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DisplayNameAttribute));
			if (displayNameAttribute != null)
			{
				xmlWriter.WriteAttributeString("display-name", displayNameAttribute.DisplayName);
			}
			DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute));
			if (descriptionAttribute != null)
			{
				xmlWriter.WriteAttributeString("description", descriptionAttribute.Description);
			}
			CategoryAttribute categoryAttribute = (CategoryAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CategoryAttribute));
			if (categoryAttribute != null)
			{
				xmlWriter.WriteAttributeString("category", categoryAttribute.Category);
			}
			ReadOnlyAttribute readOnlyAttribute = (ReadOnlyAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ReadOnlyAttribute));
			if (readOnlyAttribute != null)
			{
				xmlWriter.WriteAttributeString("read-only", XmlConvert.ToString(readOnlyAttribute.IsReadOnly));
			}
			CustomEditorAttribute customEditorAttribute = (CustomEditorAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CustomEditorAttribute));
			if (customEditorAttribute != null)
			{
				xmlWriter.WriteAttributeString("custom-editor", customEditorAttribute.Text);
				if (customEditorAttribute.GetMethodName != null)
				{
					instance.GetType().GetMethod(customEditorAttribute.GetMethodName, BindingFlags.Instance | BindingFlags.Public)?.Invoke(instance, new object[1] { xmlWriter });
				}
			}
			else
			{
				object propertyValue = null;
				try
				{
					propertyValue = propertyInfo.GetValue(instance, new object[0]);
					if (propertyValue != null)
					{
						TypeConverter converter = TypeDescriptor.GetConverter(propertyValue);
						string value = ((converter == null || !converter.CanConvertTo(typeof(string))) ? propertyValue.ToString() : converter.ConvertToString(propertyValue));
						xmlWriter.WriteAttributeString("value", value);
						xmlWriter.WriteAttributeString("default-value", value);
					}
				}
				catch (Exception ex)
				{
					LogHelper.Instance.Log($"Walk tree: caught an exception {ex.Message}", LogEntryType.Error);
				}
				if (!Attribute.IsDefined(propertyInfo, typeof(ExcludeTypeAttribute)))
				{
					xmlWriter.WriteAttributeString("type", propertyInfo.PropertyType.AssemblyQualifiedName);
				}
				if (Attribute.IsDefined(propertyInfo, typeof(RecurseAttribute)) && propertyValue != null)
				{
					WalkObjectTree(propertyValue, xmlWriter);
				}
				else
				{
					ValidValueListProviderAttribute validValueListProviderAttribute = (ValidValueListProviderAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ValidValueListProviderAttribute));
					if (validValueListProviderAttribute != null)
					{
						MethodInfo getMethod = instance.GetType().GetMethod(validValueListProviderAttribute.MethodName, BindingFlags.Instance | BindingFlags.Public);
						if (getMethod != null && getMethod.Invoke(instance, null) is string[] validValues)
						{
							xmlWriter.WriteAttributeString("valid-value-count", validValues.Length.ToString());
							foreach (string validValue in validValues)
							{
								xmlWriter.WriteStartElement("valid");
								xmlWriter.WriteAttributeString("value", validValue);
								xmlWriter.WriteEndElement();
							}
						}
					}
				}
			}
			xmlWriter.WriteEndElement();
		}
	}
}
